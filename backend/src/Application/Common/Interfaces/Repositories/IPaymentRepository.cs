using Domain.Purchases.Payments;

namespace Application.Common.Interfaces.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByTransactionIdWithPurchaseAndItemsAndVariantAsync(string transactionId, CancellationToken ct);
    public Task<Payment?> GetByPurchaseIdWithPurchaseAndItemsAndVariantsAsync(Guid purchaseId, CancellationToken ct);
}
