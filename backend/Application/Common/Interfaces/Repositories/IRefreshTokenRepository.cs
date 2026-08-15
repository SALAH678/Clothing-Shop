using Domain.Users.RefreshTokens;

namespace Application.Common.Interfaces.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    public Task<RefreshToken?> GetByValueAsync(string value, CancellationToken cancellationToken = default);
}
