using Application.Common.Interfaces.Repositories;
using Domain.Purchases.PurchaseItems;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;

namespace infrastructure.Repositories;

public sealed class PurchaseItemRepository(AppDbContext context) : Repository<PurchaseItem>(context), IPurchaseItemRepository
{
}
