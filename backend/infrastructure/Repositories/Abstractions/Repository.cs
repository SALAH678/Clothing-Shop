using Application.Common.Interfaces.Repositories;
using Domain.Common;
using infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Repositories.Abstractions;

public abstract class Repository<TEntity>(AppDbContext Context) : IRepository<TEntity>
        where TEntity : Entity
{
    protected readonly AppDbContext _Context = Context;

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default) =>
        await _Context.Set<TEntity>()
        .AsNoTracking().
        ToListAsync(ct);

    public virtual async ValueTask<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _Context.Set<TEntity>()
        .FindAsync([id], ct);

    public void Create(TEntity entity) => _Context.Set<TEntity>().Add(entity);

    public void Update(TEntity entity) => _Context.Set<TEntity>().Update(entity);

    public void Delete(TEntity entity) => _Context.Set<TEntity>().Remove(entity);

}
