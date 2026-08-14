using Application.Common.Interfaces.Repositories;
using Domain.Products.Variants;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;

namespace infrastructure.Repositories;

public sealed class VariantRepository(AppDbContext context) : Repository<Variant>(context), IVariantRepository
{
}
