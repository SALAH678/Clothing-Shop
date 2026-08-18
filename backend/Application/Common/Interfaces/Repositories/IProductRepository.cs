using Application.Common.Models;
using Application.Features.Products.Queries.GetProducts;
using Domain.Products;

namespace Application.Common.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    public Task<PaginatedList<Product>> GetProductsAsync(Guid categoryId, ProductFilter productFilter, CancellationToken cancellationToken = default);
}
