using Application.Common.Errors;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Features.Authentications.Dtos;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
using Domain.Users;
using Domain.Users.Accounts;
using Domain.Users.RefreshTokens;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Authentications.Commands.LogInExternalAuth;


public class LogInExternalAuthCommandHandler(IOAuthService oAuthService, IUnitOfWork unitOfWork, ITokenProvider tokenProvider,
    ITokenHasherService tokenHasherService, IMapper mapper, ILogger<LogInExternalAuthCommand> logger) : IRequestHandler<LogInExternalAuthCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(LogInExternalAuthCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Google login attempt started");

        // 1. Validate Google ID token

        OAuthUserInfo googleInfo;

        try
        {
            googleInfo = await oAuthService.ValidateGoogleTokenAsync(request.IdToken, cancellationToken);
        }
        catch (InvalidOAuthTokenException ex)
        {
            logger.LogWarning(ex, "Google ID token validation failed");

            return ApplicationErrors.InvalidGoogleId;
        }

        if (!googleInfo.EmailVerified)
        {
            logger.LogWarning("Google login rejected - unverified email {Email}", googleInfo.Email);

            return ApplicationErrors.EmailNotVerified;
        }

        // 2. Look for existing Google Account

        var account = await unitOfWork.Accounts.GetByProviderAsync(googleInfo.ProviderAccountId, cancellationToken);

        User? user;

        if (account is not null)
        {
            // Google account already linked

            user = await unitOfWork.Users.GetByIdAsync( account.UserId, cancellationToken);

            if (user is null)
            {
                logger.LogError("Orphaned Google account detected. AccountId {AccountId}, UserId {UserId}",
                    account.Id, account.UserId);

                return ApplicationErrors.UserNotFound;
            }
        }
        else
        {
            // Google account is NOT linked

            var emailResult = Email.Create(googleInfo.Email);

            if (emailResult.IsError)
                return emailResult.TopError;

            user = await unitOfWork.Users.GetByEmailAsync(emailResult.Value, cancellationToken);

            if (user is null)
            {
                return ApplicationErrors.UserNotFound;
            }

            // Automatically link Google to existing User

            var accountResult = Account.Create(user.Id, "Google", googleInfo.ProviderAccountId);

            if (accountResult.IsError)
                return accountResult.TopError;

            unitOfWork.Accounts.Create(accountResult.Value);

            logger.LogInformation("Google account automatically linked to UserId {UserId}", user.Id);
        }

        // 3. Generate JWT + Refresh Token

        var tokensResult = tokenProvider.GenerateJwtToken(
            user.Id.ToString(),
            user.Email.Value,
            user.UserRole.ToString());

        if (tokensResult.IsError)
        {
            logger.LogError("Token generation failed during Google login for UserId {UserId}: {Error}", user.Id, tokensResult.TopError);

            return tokensResult.TopError;
        }

        var refreshTokenExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(7);

        var finalTokens = tokensResult.Value with
        {
            RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc
        };

        if (finalTokens.RefreshToken is null)
        {
            logger.LogError("Generated refresh token is null for UserId {UserId}", user.Id);

            return ApplicationErrors.LoginFailed;
        }

        // Store only the hash.
        var hashedToken = tokenHasherService.HashToken(finalTokens.RefreshToken);

        var refreshTokenResult = RefreshToken.Create(
            userId: user.Id,
            value: hashedToken,
            expiresAtUtc: refreshTokenExpiresAtUtc);

        if (refreshTokenResult.IsError)
            return refreshTokenResult.TopError;

        unitOfWork.RefreshTokens.Create(refreshTokenResult.Value);

        // 4. Save changes to the database

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Google login succeeded for UserId {UserId}", user.Id);

        return new AuthResponse(
            User: mapper.Map<UserDto>(user),
            Tokens: finalTokens);
    }
}