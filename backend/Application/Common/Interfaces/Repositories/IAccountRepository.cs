using Domain.Common.ValueObjects.Email;
using Domain.Users.Accounts;

namespace Application.Common.Interfaces.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    public Task<Account?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
