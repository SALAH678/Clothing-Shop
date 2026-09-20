using Domain.Users.Accounts;

namespace Application.Common.Interfaces.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Account?> GetByProviderAsync(string providerAccountId, CancellationToken cancellationToken = default);
}
