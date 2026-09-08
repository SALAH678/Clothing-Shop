using Domain.Common.ValueObjects.Email;
using Domain.Users;
using Domain.Users.Accounts;

namespace Application.Common.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailWithTrackingAsync(Email email, CancellationToken cancellationToken = default);
}
