using Domain.Users.RefreshTokens;

namespace Application.Common.Interfaces.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    public Task<RefreshToken?> GetByValueAsync(string value, CancellationToken cancellationToken = default);
    public Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
