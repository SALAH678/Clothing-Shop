using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Features.Identity;
using Domain.Common.Results;
using Domain.Users;
using Domain.Users.RefreshTokens;
using Microsoft.Extensions.Logging;

namespace Application.Common.Services;

public class AuthenticationTokenService(ITokenProvider tokenProvider, ITokenHasherService tokenHasherService,
    IUnitOfWork unitOfWork, ILogger<AuthenticationTokenService> logger) : IAuthenticationTokenService
{
    public async Task<Result<TokenResponse>> CreateAsync(User user, CancellationToken cancellationToken)
    {
        var tokens = tokenProvider.GenerateJwtToken(user.Id.ToString(), user.Email.Value, user.UserRole.ToString());

        if (tokens.IsError)
        {
            logger.LogError("Token generation failed for UserId {UserId}: {Error}", user.Id, tokens.TopError);

            return tokens.TopError;
        }

        var refreshTokenExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(7);

        var finalTokens = tokens.Value with
        {
            RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc
        };

        if (finalTokens.RefreshToken is null)
        {
            logger.LogError("Generated refresh token is null for UserId {UserId}", user.Id);

            return ApplicationErrors.LoginFailed;
        }

        var hashedToken = tokenHasherService.HashToken(finalTokens.RefreshToken);

        var refreshToken = RefreshToken.Create(userId: user.Id, value: hashedToken, expiresAtUtc: refreshTokenExpiresAtUtc);

        if (refreshToken.IsError)
        {
            logger.LogError("Refresh token creation failed for UserId {UserId}: {Error}", user.Id, refreshToken.TopError);

            return refreshToken.TopError;
        }

        unitOfWork.RefreshTokens.Create(refreshToken.Value);

        return finalTokens;
    }
}