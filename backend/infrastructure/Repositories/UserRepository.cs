using Application.Common.Interfaces.Repositories;
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
}
