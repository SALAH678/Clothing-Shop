using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Common.ValueObjects.Email;
using Domain.Users;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Repositories;

public sealed class UserRepository(AppDbContext context) : Repository<User>(context), IUserRepository
{
    public async Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default) =>
            await _Context.Users
            .AnyAsync(c => c.Email == email, cancellationToken);

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        await _Context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);

    public async Task<User?> GetByEmailWithTrackingAsync(Email email, CancellationToken cancellationToken = default) =>
        await _Context.Users
        .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);

    public async Task<int> GetTotalUsersNumber(CancellationToken cancellationToken = default) =>
        await _Context.Users.CountAsync(cancellationToken);

    public async Task<PaginatedList<User>> GetPaginatedUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _Context.Users.AsNoTracking();
        var itemsNumber = await query.CountAsync(cancellationToken);
        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        PaginatedList<User> result = new PaginatedList<User>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = itemsNumber,
            TotalPages = (int)Math.Ceiling((double)itemsNumber / pageSize),//Math.Ceiling(3.1); the result is 4
            Items = users
        };
        return result;
    }
}
