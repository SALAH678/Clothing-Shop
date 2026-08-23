using Application.Common.Interfaces.Services;
using Application.Features.Authentications.Dtos;
using Google.Apis.Auth;

namespace infrastructure.Services;

//public class OAuthService : IOAuthService
//{
//    public async Task<ExternalUserInfo> AuthenticateWithGoogleAsync(string idToken, CancellationToken cancellationToken = default)
//    {
//        if (string.IsNullOrWhiteSpace(idToken))
//        {
//            throw new ArgumentException("Google ID token is required.", nameof(idToken));
//        }

//        if (string.IsNullOrWhiteSpace(clientId))
//        {
//            throw new ArgumentException("Google client ID is required.", nameof(clientId));
//        }

//        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
//        {
//            Audience = new[] { clientId }
//        };

//        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

//        if (string.IsNullOrWhiteSpace(payload.Email))
//        {
//            throw new InvalidOperationException("Google token does not contain an email claim.");
//        }

//        return new ExternalUserInfo(
//            Provider: "Google",
//            ProviderUserId: payload.Subject ?? payload.Email,
//            Email: payload.Email,
//            FirstName: payload.GivenName ?? string.Empty,
//            LastName: payload.FamilyName ?? string.Empty,
//            EmailVerified: payload.EmailVerified ?? false);
//    }
//}
