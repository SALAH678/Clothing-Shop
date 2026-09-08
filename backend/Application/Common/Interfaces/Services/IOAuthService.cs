using Application.Features.Authentications.Dtos;

namespace Application.Common.Interfaces.Services;

public interface IOAuthService
{
    Task<OAuthUserInfo> ValidateGoogleTokenAsync(string idToken, CancellationToken ct);
}

public record OAuthUserInfo(
    string Email,
    bool EmailVerified,
    string FirstName,
    string LastName,
    string ProviderAccountId);
