using Application.Common.Interfaces.Repositories;
using Domain.Users.RefreshTokens;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Repositories;

public sealed class RefreshTokenRepository(AppDbContext context) : Repository<RefreshToken>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByValueAsync(string value, CancellationToken cancellationToken = default) =>
        await _Context.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.Value == value, cancellationToken);

    public async Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => 
        await _Context.RefreshTokens
        .Where(rt => rt.UserId == userId && !rt.IsRevoked)
        .ToListAsync(cancellationToken);
}
