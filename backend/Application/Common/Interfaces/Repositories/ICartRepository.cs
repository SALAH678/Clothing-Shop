using Domain.Carts;

namespace Application.Common.Interfaces.Repositories;

public interface ICartRepository : IRepository<Cart>
{
    public Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}