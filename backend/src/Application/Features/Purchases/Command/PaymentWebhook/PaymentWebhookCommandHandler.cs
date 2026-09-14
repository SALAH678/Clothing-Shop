using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Repositories;
using Domain.Common.Results;
using Domain.Purchases.Enum;
using Domain.Purchases.Payments.Enum;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Purchases.Command.PaymentWebhook;

public class PaymentWebhookCommandHandler(IUnitOfWork unitOfWork, IPaymentRepository paymentRepository,
    ILogger<PaymentWebhookCommandHandler> logger, IEmailJob emailJob) : IRequestHandler<PaymentWebhookCommand, Result<Updated>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPaymentRepository _paymentRepository = paymentRepository;
    private readonly ILogger<PaymentWebhookCommandHandler> _logger = logger;
    private readonly IEmailJob _emailJob = emailJob;

    public async Task<Result<Updated>> Handle(PaymentWebhookCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Processing webhook for checkout {CheckoutId}, event {EventType}, status {Status}", request.CheckoutId,
            request.EventType, request.Status);

        // 1. Look up the Payment by the Chargily checkout id we stored earlier
        var payment = await _paymentRepository.GetByTransactionIdWithPurchaseAndItemsAndVariantAsync(request.CheckoutId, ct);

        if (payment is null)
        {
            _logger.LogWarning("Webhook received for unknown checkout {CheckoutId}. Ignoring.", request.CheckoutId);
            return ApplicationErrors.PaymentNotFound;
        }

        // 2. Idempotency guard — Chargily may deliver the same event more than once
        if (payment.Status == PaymentStatus.Paid || payment.Status == PaymentStatus.Failed)
        {
            _logger.LogInformation("Webhook for checkout {CheckoutId} already processed (current status: {Status}). Skipping.",
                request.CheckoutId, payment.Status);
            return Result.Updated;
        }

        // 3. Map Chargily's status to my own PaymentStatus
        var isPaid = request.Status.Equals("paid", StringComparison.OrdinalIgnoreCase);
        var isTerminalFailure = request.Status is "failed" or "expired" or "canceled";

        if (!isPaid && !isTerminalFailure)
        {
            _logger.LogInformation("Webhook for checkout {CheckoutId} has non-terminal status {Status}. No action taken.",
                request.CheckoutId, request.Status);
            return Result.Updated;
        }

        // 4. Update Payment status
        var paymentUpdateResult = isPaid ? payment.UpdateStatus(PaymentStatus.Paid) : payment.UpdateStatus(PaymentStatus.Failed);

        if (paymentUpdateResult.IsError)
        {
            _logger.LogError("Failed to update Payment {PaymentId} status for checkout {CheckoutId}: {Error}",
                payment.Id, request.CheckoutId, paymentUpdateResult.TopError.Description);
            return paymentUpdateResult.TopError;
        }

        var purchaseId = payment.PurchaseId;

        // 5. Handle successful or failed payment
        if (isPaid)
        {
            if (payment.Purchase.Origin == PurchaseOrigin.Cart)
            {
                var cart = await _unitOfWork.Carts.GetByUserIdWithItemsAsync(payment.Purchase.UserId, ct);

                if (cart is not null)
                {
                    foreach (var item in cart.Items)
                        _unitOfWork.CartItems.Delete(item);

                    _logger.LogInformation("Cleared cart for user {UserId} after purchase {PurchaseId}", payment.Purchase.UserId, purchaseId);
                }
                else
                    _logger.LogWarning("No cart found for user {UserId} after purchase {PurchaseId}.", payment.Purchase.UserId, purchaseId);
            }
        }
        else
        {
            foreach (var item in payment.Purchase.Items)
            {
                var restoreResult = item.Variant.IncreaseStock(item.Quantity);
                if (restoreResult.IsError)
                {
                    _logger.LogError("Failed to restore stock for variant {VariantId} by {Quantity} on purchase {PurchaseId}: {Error}",
                        item.VariantId, item.Quantity, purchaseId, restoreResult.TopError.Description);
                }
            }

            _logger.LogInformation("Purchase {PurchaseId} failed. Stock restored for {ItemCount} item(s).", purchaseId, payment.Purchase.Items.Count);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        if(isPaid)
        {
            List<PurchaseNotificationItemPayload> notificationItems = payment.Purchase.Items.Select(item => new PurchaseNotificationItemPayload
            {
                ProductName = item.Variant.Product.Name,
                Size = item.Variant.Size,
                Color = item.Variant.Color,
                Price = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList();

            await _emailJob.ScheduleSendPurchaseNotificationAsync(payment.PurchaseId, payment.Purchase.UserId, notificationItems, payment.Purchase.TotalAmount, ct);
            _logger.LogInformation("Enqueued admin notification job for purchase {PurchaseId}", payment.PurchaseId);
        }
         
        _logger.LogInformation("Webhook processing completed for checkout {CheckoutId}, purchase {PurchaseId}", request.CheckoutId, purchaseId);

        return Result.Updated;
    }
}
