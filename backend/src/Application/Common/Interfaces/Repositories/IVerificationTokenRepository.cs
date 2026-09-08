using Domain.Users.VerificationTokens;
using Domain.Users.VerificationTokens.Enum;

namespace Application.Common.Interfaces.Repositories;

public interface IVerificationTokenRepository : IRepository<VerificationToken>
{
    Task<VerificationToken?> GetByUserIdAsync(Guid userId, VerificationTokenType tokenType, CancellationToken cancellationToken);
}
