using Application.Common.Models;
using Domain.Common.ValueObjects.Email;
using Domain.Users;

namespace Application.Common.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailWithTrackingAsync(Email email, CancellationToken cancellationToken = default);
    Task<int> GetTotalUsersNumber(CancellationToken cancellationToken = default);
    Task<PaginatedList<User>> GetPaginatedUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
