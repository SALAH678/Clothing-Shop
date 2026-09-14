using Application.Common.Interfaces.Repositories;
using Domain.Purchases.Payments;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Repositories;

public sealed class PaymentRepository(AppDbContext context) : Repository<Payment>(context), IPaymentRepository
{
    public async Task<Payment?> GetByTransactionIdWithPurchaseAndItemsAndVariantAsync(string transactionId, CancellationToken ct) => 
        await _Context.Payments
            .Include(p => p.Purchase)
                .ThenInclude(pu => pu.Items)
                    .ThenInclude(i => i.Variant)
                        .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, ct);

    public async Task<Payment?> GetByPurchaseIdWithPurchaseAndItemsAndVariantsAsync(Guid purchaseId, CancellationToken ct) =>
        await _Context.Payments
            .Include(p => p.Purchase)
                .ThenInclude(pu => pu.Items)
                    .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId, ct);
}
