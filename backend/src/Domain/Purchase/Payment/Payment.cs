using Domain.Common;
using Domain.Common.Results;
using Domain.Purchases.Payments.Enum;

namespace Domain.Purchases.Payments;

public class Payment : AuditableEntity
{
    public Guid PurchaseId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? TransactionId { get; private set; }

    public Purchase Purchase { get; private set; } = null!;

    protected Payment()
    {
    }

    protected Payment(Guid purchaseId, decimal amount, PaymentStatus status, string? transactionId)
        : base(Guid.Empty)
    {
        PurchaseId = purchaseId;
        Amount = amount;
        Status = status;
        TransactionId = transactionId;
    }

    public static Result<Payment> Create(Guid purchaseId, decimal amount, PaymentStatus status, string? transactionId = null)
    {
        Error? error = Validate(purchaseId, amount, status, transactionId);

        if (error is not null)
            return error.Value;

        return new Payment(purchaseId, amount, status, transactionId?.Trim());
    }

    public void AddPurchase(Purchase purchase)
    {
        Purchase = purchase;
    }

    public Result<Updated> AttachCheckout(string? checkoutId)
    {
        if (string.IsNullOrWhiteSpace(checkoutId))
            return PaymentErrors.TransactionIdRequired;

        TransactionId = checkoutId.Trim();

        return Result.Updated;
    }

    public Result<Updated> UpdateStatus(PaymentStatus status)
    {
        if (!System.Enum.IsDefined(status))
            return PaymentErrors.InvalidStatus;

        Status = status;

        return Result.Updated;
    }

    public Result<Updated> UpdateTransactionId(string? transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            return PaymentErrors.TransactionIdRequired;

        TransactionId = transactionId.Trim();

        return Result.Updated;
    }

    private static Error? Validate(Guid purchaseId, decimal amount, PaymentStatus status, string? transactionId)
    {
        if (purchaseId == Guid.Empty)
            return PaymentErrors.PurchaseIdRequired;

        if (amount < 0)
            return PaymentErrors.InvalidAmount;

        if (!System.Enum.IsDefined(status))
            return PaymentErrors.InvalidStatus;

        if (transactionId is not null && string.IsNullOrWhiteSpace(transactionId))
            return PaymentErrors.TransactionIdRequired;

        return null;
    }
}
