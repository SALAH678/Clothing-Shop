using Application.Common.Interfaces.Repositories;
using Domain.Categories;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Repositories;

public sealed class CategoryRepository(AppDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public async Task<bool> ExistsAsync(Guid categoryId, CancellationToken cancellationToken = default) => 
        await _Context.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken);

    public override async ValueTask<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _Context.Categories
        .Include(c => c.Subcategories)
        .FirstOrDefaultAsync(c => c.Id == id, ct);

    public override async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
        await _Context.Categories
        .Include(c => c.Subcategories)
        .AsNoTracking()
        .ToListAsync(ct);
}
