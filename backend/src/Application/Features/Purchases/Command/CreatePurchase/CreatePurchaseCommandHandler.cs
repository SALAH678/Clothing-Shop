using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using Domain.Common.ValueObjects.PhoneNumber;
using Domain.Purchases;
using Domain.Purchases.Payments;
using Domain.Purchases.Payments.Enum;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Purchases.Command.CreatePurchase;

public class CreatePurchaseCommandHandler(IUnitOfWork unitOfWork, IVariantRepository variantRepository, IPaymentGatewayService paymentGateway,
    ILogger<CreatePurchaseCommandHandler> logger, IUser user) : IRequestHandler<CreatePurchaseCommand, Result<CreatePurchaseResult>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IVariantRepository _variantRepository = variantRepository;
    private readonly IPaymentGatewayService _paymentGateway = paymentGateway;
    private readonly ILogger<CreatePurchaseCommandHandler> _logger = logger;
    private readonly IUser _user = user;

    public async Task<Result<CreatePurchaseResult>> Handle(CreatePurchaseCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Starting purchase creation for user {UserId} with {ItemCount} item(s)",
            _user.UserId, request.Items.Count);

        // 1. Load variants, validate stock, snapshot prices
        var variantIds = request.Items.Select(i => i.VariantId).ToList();

        var variants = await _variantRepository.GetByIdsWithProductAsync(variantIds, ct);

        var enrichedItems = new List<(Guid VariantId, int Quantity, decimal UnitPrice)>();

        foreach (var item in request.Items)
        {
            var variant = variants.FirstOrDefault(v => v.Id == item.VariantId);
            if (variant is null)
            {
                _logger.LogWarning("Variant {VariantId} not found for user {UserId}", item.VariantId, _user.UserId);

                return Error.NotFound(
                    code: "VARIANT_NOT_FOUND",
                    description: $"Variant {item.VariantId} not found.");
            }

            if (variant.StockQuantity < item.Quantity)
            {
                _logger.LogWarning("Insufficient stock for variant {VariantId}: requested {Requested}, available {Available}, user {UserId}",
                    item.VariantId, item.Quantity, variant.StockQuantity, _user.UserId);

                return Error.Conflict(
                    code: "INSUFFICIENT_STOCK",
                    description: $"Not enough stock for variant {item.VariantId}.");
            }

            var unitPrice = variant.Product.BasePrice - (variant.Product.Discount ?? 0m);

            enrichedItems.Add((item.VariantId, item.Quantity, unitPrice));
        }

        _logger.LogInformation("Stock validated for all {ItemCount} item(s), user {UserId}", enrichedItems.Count, _user.UserId);

        // 2. Create Purchase + PurchaseItems + Payment, commit locally first
        var phoneNumberResult = PhoneNumber.Create(request.CustomerPhone);
        if (phoneNumberResult.IsError)
        {
            _logger.LogWarning( "Invalid phone number for user {UserId}: {Error}", _user.UserId, phoneNumberResult.TopError.Description);
            return phoneNumberResult.TopError;
        }

        var purchase = Purchase.Create(_user.UserId, phoneNumberResult.Value, request.CustomerAddress);

        foreach(var item in enrichedItems)
        {
            var addItemResult = purchase.Value.AddItem(item.VariantId, item.Quantity, item.UnitPrice);
            if (addItemResult.IsError)
            {
                _logger.LogWarning("Failed to add item {VariantId} to purchase {PurchaseId} for user {UserId}: {Error}",
                    item.VariantId, purchase.Value.Id, _user.UserId, addItemResult.TopError.Description);
                return addItemResult.TopError;
            }
        }

        var payment = Payment.Create(purchase.Value.Id, purchase.Value.TotalAmount, PaymentStatus.Pending);

        _unitOfWork.Purchases.Create(purchase.Value);
        _unitOfWork.Payments.Create(payment.Value);

        // 3. Decrease stock for each variant
        foreach (var item in purchase.Value.Items)
        {
            var decrementResult = item.Variant.DecreaseStock(item.Quantity);
            if (decrementResult.IsError)
            {
                _logger.LogError("Failed to decrement stock for variant {VariantId} by {Quantity} on purchase {PurchaseId}: {Error}", item.VariantId,
                    item.Quantity, purchase.Value.Id, decrementResult.TopError.Description);
                return decrementResult.TopError;
            }
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save purchase {PurchaseId} and payment {PaymentId} for user {UserId}",
                purchase.Value.Id, payment.Value.Id, _user.UserId);
            return Error.Failure(
                code: "PURCHASE_SAVE_FAILED",
                description: "Failed to save purchase. Please try again.");
        }

        _logger.LogInformation("Created Purchase {PurchaseId} and Payment {PaymentId} for user {UserId}, total {TotalAmount}",
            purchase.Value.Id, payment.Value.Id, _user.UserId, purchase.Value.TotalAmount);

        // 4. Call Chargily
        CheckoutResult checkout;
        try
        {
            checkout = await _paymentGateway.CreateCheckout(purchase.Value.Id, purchase.Value.TotalAmount);
        }
        catch (PaymentGatewayException ex)
        {
            _logger.LogWarning(ex, "Checkout creation failed for purchase {PurchaseId}, user {UserId}. Purchase left as PendingPayment for retry.",
                purchase.Value.Id, _user.UserId);
            return Error.Failure(
                code: "Checkout_Failed",
                description: "Could not initiate payment. Please try again.");
        }

        _logger.LogInformation("Chargily checkout {CheckoutId} created for purchase {PurchaseId}", checkout.CheckoutId, purchase.Value.Id);

        // 5. Attach the checkout id
        var attachResult = payment.Value.AttachCheckout(checkout.CheckoutId);
        if (attachResult.IsError)
        {
            _logger.LogError("Failed to attach checkout {CheckoutId} to payment {PaymentId} for user {UserId}: {Error}",
                checkout.CheckoutId, payment.Value.Id, _user.UserId, attachResult.TopError.Description);
            return Error.Failure(
                code: "Checkout_Attach_Failed",
                description: "Could not attach checkout to payment. Please try again.");
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save checkout {CheckoutId} for payment {PaymentId} and purchase {PurchaseId} for user {UserId}",
                checkout.CheckoutId, payment.Value.Id, purchase.Value.Id, _user.UserId);
            return Error.Failure(
                code: "Checkout_Save_Failed",
                description: "Failed to save checkout. Please try again.");
        }

        _logger.LogInformation("Purchase {PurchaseId} creation completed successfully for user {UserId}",
            purchase.Value.Id, _user.UserId);

        return new CreatePurchaseResult(purchase.Value.Id, checkout.CheckoutUrl.ToString());
    }
}
