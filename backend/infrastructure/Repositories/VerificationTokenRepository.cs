using Application.Common.Interfaces.Repositories;
using Domain.Users.VerificationTokens;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Repositories;

public sealed class VerificationTokenRepository(AppDbContext context) : Repository<VerificationToken>(context), IVerificationTokenRepository
{
    public async Task<VerificationToken?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) => 
        await _Context.VerificationTokens
        .FirstOrDefaultAsync(vt => vt.UserId == userId, cancellationToken);
}
