using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Features.Products.Queries.GetProducts;
using Domain.Products;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Repositories;

public sealed class ProductRepository(AppDbContext context) : Repository<Product>(context), IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        await _Context.Products
            .Include(product => product.Variants)
            .Include(product => product.Images)
            .Where(product => product.CategoryId == categoryId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public override async ValueTask<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _Context.Products
            .Include(product => product.Variants)
            .Include(product => product.Images)
            .FirstOrDefaultAsync(product => product.Id == id, ct);

    public async Task<PaginatedList<Product>> GetProductsAsync(Guid categoryId, ProductFilter productFilter,
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _Context.Products.AsNoTracking().AsQueryable();

        query = query.Where(product => product.CategoryId == categoryId);

        if(!string.IsNullOrWhiteSpace(productFilter.Search))
            query = query.Where(product => product.Name.ToLower().Contains(productFilter.Search.ToLower()));

        if (productFilter.MinPrice.HasValue)
            query = query.Where(product => product.BasePrice >= productFilter.MinPrice.Value);

        if (productFilter.MaxPrice.HasValue)
            query = query.Where(product => product.BasePrice <= productFilter.MaxPrice.Value);

        //if(!string.IsNullOrWhiteSpace(productFilter.size))
        //    query = query.Where(product => product.Variants.Any(variant => variant.Size == productFilter.size));

        bool hasSize = !string.IsNullOrWhiteSpace(productFilter.size);
        bool hasColor = !string.IsNullOrWhiteSpace(productFilter.color);

        if (hasSize || hasColor)
        {
            string? targetSize = productFilter.size?.Trim().ToLower();
            string? targetColor = productFilter.color?.Trim().ToLower();

            query = query.Where(product => product.Variants.Any(variant =>
                (!hasSize || variant.Size!.ToLower() == targetSize) &&
                (!hasColor || variant.Color!.ToLower() == targetColor)));
        }

        var itemsNumber = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(productFilter.SortBy) && productFilter.SortBy.Equals("price", StringComparison.OrdinalIgnoreCase))
            query = productFilter.Descending ? query.OrderByDescending(p => p.BasePrice)
                : query.OrderBy(p => p.BasePrice);
        else
            query = productFilter.Descending ? query.OrderByDescending(p => p.CreatedAtUtc)
                : query.OrderBy(p => p.CreatedAtUtc);

        var products = await query
            .Include(product => product.Variants)
            .Include(product => product.Images)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        PaginatedList<Product> result = new PaginatedList<Product>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = itemsNumber,
            TotalPages = (int)Math.Ceiling((double)itemsNumber / pageSize),//Math.Ceiling(3.1); the result is 4
            Items = products
        };

        return result;
    }
}
