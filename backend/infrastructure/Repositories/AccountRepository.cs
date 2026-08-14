using Application.Common.Interfaces.Repositories;
using Domain.Users.Accounts;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;

namespace infrastructure.Repositories;

public sealed class AccountRepository(AppDbContext context) : Repository<Account>(context), IAccountRepository
{
}
