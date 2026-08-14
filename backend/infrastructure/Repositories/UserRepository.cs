using Application.Common.Interfaces.Repositories;
using Domain.Users;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;

namespace infrastructure.Repositories;

public sealed class UserRepository(AppDbContext context) : Repository<User>(context), IUserRepository
{
}
