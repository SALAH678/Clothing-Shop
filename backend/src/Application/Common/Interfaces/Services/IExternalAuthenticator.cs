using Domain.Common.Results;
using Domain.Users;

namespace Application.Common.Interfaces.Services;

public interface IExternalAuthenticator
{
    Task<Result<User>> AuthenticateAsync(string idToken, string? phoneNumber, CancellationToken cancellationToken);
}
