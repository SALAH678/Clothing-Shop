using Application.Common.Interfaces.Repositories;
using Domain.Users.Accounts;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Repositories;

public sealed class AccountRepository(AppDbContext context) : Repository<Account>(context), IAccountRepository
{
    public async Task<Account?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _Context.Accounts
        .AsNoTracking()
        .FirstOrDefaultAsync(a => a.UserId == userId && a.Provider == "local", cancellationToken);
}
