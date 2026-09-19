using Domain.Categories;

namespace Application.Common.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    public Task<bool> ExistsAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<int> GetTotalCategoriesNumberAsync(CancellationToken cancellationToken = default);
}
