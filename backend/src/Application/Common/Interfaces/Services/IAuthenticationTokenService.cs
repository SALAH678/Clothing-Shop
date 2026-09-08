using Application.Features.Identity;
using Domain.Common.Results;
using Domain.Users;

namespace Application.Common.Interfaces.Services;

public interface IAuthenticationTokenService
{
    Task<Result<TokenResponse>> CreateAsync(User user, CancellationToken cancellationToken);
}
