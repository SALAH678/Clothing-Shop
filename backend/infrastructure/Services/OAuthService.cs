using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace infrastructure.Services;

public class OAuthService(IConfiguration configuration) : IOAuthService
{
    private readonly IConfiguration _config = configuration;

    public async Task<OAuthUserInfo> ValidateGoogleTokenAsync(string idToken, CancellationToken ct)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Authentication:Google:ClientId"] }
            });

            return new OAuthUserInfo(
                payload.Email,
                payload.EmailVerified,
                payload.GivenName ?? string.Empty,
                payload.FamilyName ?? string.Empty,
                payload.Subject);
        }
        catch (InvalidJwtException ex)
        {
            throw new InvalidOAuthTokenException("Google ID token validation failed.", ex);
        }
    }
}
