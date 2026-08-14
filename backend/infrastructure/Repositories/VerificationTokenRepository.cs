using Application.Common.Interfaces.Repositories;
using Domain.Users.VerificationTokens;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;

namespace infrastructure.Repositories;

public sealed class VerificationTokenRepository(AppDbContext context) : Repository<VerificationToken>(context), IVerificationTokenRepository
{
}
