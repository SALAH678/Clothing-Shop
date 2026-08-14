using Application.Interfaces.Repositories;
using Domain.Purchases;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;

namespace infrastructure.Repositories;

public sealed class PurchaseRepository(AppDbContext context) : Repository<Purchase>(context), IPurchaseRepository
{
}
