using Application.Common.Models;
using Application.Features.Purchases.Dtos;
using Domain.Purchases;

namespace Application.Common.Interfaces.Repositories;

public interface IPurchaseRepository : IRepository<Purchase>
{
    Task<PaginatedList<PurchaseDto>> GetAllPurchasesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalPurchasesNumberAsync(CancellationToken cancellationToken = default);
}
