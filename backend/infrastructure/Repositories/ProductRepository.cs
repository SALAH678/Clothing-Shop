using Application.Common.Interfaces.Repositories;
using Domain.Products;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;

namespace infrastructure.Repositories;

public sealed class ProductRepository(AppDbContext context) : Repository<Product>(context), IProductRepository
{
}
