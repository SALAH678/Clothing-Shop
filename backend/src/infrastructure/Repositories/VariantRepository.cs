using Application.Common.Interfaces.Repositories;
using Domain.Products.Variants;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Repositories;

public sealed class VariantRepository(AppDbContext context) : Repository<Variant>(context), IVariantRepository
{
    public override async ValueTask<Variant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
    await _Context.Variants
        .Include(v => v.Product)
        .ThenInclude(p => p.Images)
        .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
}
