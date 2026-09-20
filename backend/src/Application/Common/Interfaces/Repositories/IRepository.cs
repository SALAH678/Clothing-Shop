using Domain.Common;

namespace Application.Common.Interfaces.Repositories;

public interface IRepository<TEntity>
    where TEntity : Entity
{
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);
    ValueTask<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TEntity>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    void Create(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}
