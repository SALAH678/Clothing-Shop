using Application.Common.Errors;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using Domain.Purchases.Payments.Enum;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Purchases.Command.RetryPurchase;

public class RetryPurchaseCommandHandler(IUnitOfWork unitOfWork, IPaymentRepository paymentRepository, IUser currentUser,
    IPaymentGatewayService paymentGateway, ILogger<RetryPurchaseCommandHandler> logger) : IRequestHandler<RetryPurchaseCommand, Result<RetryPurchaseResult>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPaymentRepository _paymentRepository = paymentRepository;
    private readonly IUser _currentUser = currentUser;
    private readonly IPaymentGatewayService _paymentGateway = paymentGateway;
    private readonly ILogger<RetryPurchaseCommandHandler> _logger = logger;

    public async Task<Result<RetryPurchaseResult>> Handle(RetryPurchaseCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Starting payment retry for purchase {PurchaseId} by user {UserId}", request.PurchaseId, _currentUser.UserId);

        // 1. Load payment with purchase, items and variants
        var payment = await _paymentRepository.GetByPurchaseIdWithPurchaseAndItemsAndVariantsAsync(request.PurchaseId, ct);

        if (payment is null)
        {
            _logger.LogWarning("Payment not found for purchase {PurchaseId}", request.PurchaseId);
            return ApplicationErrors.PaymentNotFound;
        }

        // 2. Make sure the purchase belongs to the current user
        if (payment.Purchase.UserId != _currentUser.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to retry payment for purchase {PurchaseId} that does not belong to them", _currentUser.UserId,
                request.PurchaseId);

            return Error.Forbidden(
                code: "PURCHASE_ACCESS_DENIED",
                description: "You do not have access to this purchase.");
        }

        // 3. Payment must be failed
        if (payment.Status != PaymentStatus.Failed)
        {
            _logger.LogWarning("Payment {PaymentId} for purchase {PurchaseId} cannot be retried because its status is {Status}",
                payment.Id, request.PurchaseId, payment.Status);

            return Error.Conflict(
                code: "PAYMENT_NOT_RETRYABLE",
                description: "This payment cannot be retried.");
        }

        // 4. Check stock again
        foreach (var item in payment.Purchase.Items)
        {
            if (item.Variant.StockQuantity < item.Quantity)
            {
                _logger.LogWarning("Insufficient stock for variant {VariantId} during retry of purchase {PurchaseId}. Requested {Requested}, available {Available}",
                    item.VariantId, request.PurchaseId, item.Quantity, item.Variant.StockQuantity);

                return Error.Conflict(
                    code: "INSUFFICIENT_STOCK",
                    description: $"Not enough stock for variant {item.VariantId}.");
            }
        }

        // 5. Reserve/decrease stock again
        foreach (var item in payment.Purchase.Items)
        {
            var decreaseResult = item.Variant.DecreaseStock(item.Quantity);

            if (decreaseResult.IsError)
            {
                _logger.LogError("Failed to decrease stock for variant {VariantId} during retry of purchase {PurchaseId}: {Error}",
                    item.VariantId, request.PurchaseId, decreaseResult.TopError.Description);
                return decreaseResult.TopError;
            }
        }

        // 6. Failed -> Pending
        var paymentUpdateResult = payment.UpdateStatus(PaymentStatus.Pending);

        if (paymentUpdateResult.IsError)
        {
            _logger.LogError("Failed to change payment {PaymentId} from Failed to Pending: {Error}",
                payment.Id, paymentUpdateResult.TopError.Description);
            return paymentUpdateResult.TopError;
        }

        _logger.LogInformation("Payment {PaymentId} reset to Pending and stock reserved for purchase {PurchaseId}",
            payment.Id, request.PurchaseId);

        // 7. Create a new Chargily checkout
        CheckoutResult checkout;

        try
        {
            checkout = await _paymentGateway.CreateCheckout(payment.PurchaseId, payment.Amount);
        }
        catch (PaymentGatewayException ex)
        {
            _logger.LogError(ex, "Failed to create new checkout for purchase {PurchaseId} during payment retry", request.PurchaseId);

            return Error.Failure(
                code: "CHECKOUT_FAILED",
                description: "Could not initiate payment. Please try again.");
        }

        _logger.LogInformation("New Chargily checkout {CheckoutId} created for purchase {PurchaseId}",
            checkout.CheckoutId, request.PurchaseId);

        // 8. Attach the new checkout ID
        var attachResult = payment.AttachCheckout(checkout.CheckoutId);

        if (attachResult.IsError)
        {
            _logger.LogError("Failed to attach checkout {CheckoutId} to payment {PaymentId}: {Error}",
                checkout.CheckoutId, payment.Id, attachResult.TopError.Description);

            return Error.Failure(
                code: "CHECKOUT_ATTACH_FAILED",
                description: "Could not attach the payment checkout.");
        }

        // 9. Save all changes
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Payment retry completed successfully for purchase {PurchaseId}", request.PurchaseId);

        // 10. Return new checkout URL
        return new RetryPurchaseResult(checkout.CheckoutUrl.ToString());
    }
}
