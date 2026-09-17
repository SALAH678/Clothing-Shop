using Application.Common.Errors;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Features.Authentications.Dtos;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
using Domain.Common.ValueObjects.PhoneNumber;
using Domain.Users;
using Domain.Users.Accounts;
using Domain.Users.RefreshTokens;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Authentications.Commands.RegisterExternalAuth;

public class RegisterExternalAuthCommandHandler(IOAuthService oAuthService, IUnitOfWork unitOfWork, ITokenProvider tokenProvider,
    ITokenHasherService tokenHasherService, IMapper mapper, ILogger<RegisterExternalAuthCommandHandler> logger) : IRequestHandler<RegisterExternalAuthCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RegisterExternalAuthCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Google registration attempt started");

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

        // Google must confirm that the email is verified.

        if (!googleInfo.EmailVerified)
        {
            logger.LogWarning("Google registration rejected - unverified email {Email}", googleInfo.Email);

            return ApplicationErrors.EmailNotVerified;
        }

        // 2. Make sure this Google account isn't already linked

        var existingAccount = await unitOfWork.Accounts.GetByProviderAsync(googleInfo.ProviderAccountId, cancellationToken);

        if (existingAccount is not null)
        {
            logger.LogInformation("Google registration rejected - account already exists");

            return ApplicationErrors.AccountAlreadyExists;
        }

        // 3. Create Email value object

        var emailResult = Email.Create(googleInfo.Email);

        if (emailResult.IsError)
            return emailResult.TopError;

        // 4. Make sure a User with this email doesn't already exist

        var existingUser = await unitOfWork.Users.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (existingUser is not null)
            return ApplicationErrors.UserAlreadyExists;

        // 5. Validate phone number

        var phoneResult = PhoneNumber.Create(request.PhoneNumber);

        if (phoneResult.IsError)
            return phoneResult.TopError;

        // 6. Create User

        var userResult = User.Create(googleInfo.FirstName, googleInfo.LastName, emailResult.Value, phoneResult.Value);

        if (userResult.IsError)
            return userResult.TopError;

        var user = userResult.Value;

        // Google already verified the email.
        user.MarkEmailVerified();

        unitOfWork.Users.Create(user);

        // 7. Create Google Account

        var accountResult = Account.Create(user.Id, "Google", googleInfo.ProviderAccountId);

        if (accountResult.IsError)
            return accountResult.TopError;

        unitOfWork.Accounts.Create(accountResult.Value);

        // 8. Generate JWT + Refresh Token

        var tokensResult = tokenProvider.GenerateJwtToken(
            user.Id.ToString(),
            user.Email.Value,
            user.UserRole.ToString());

        if (tokensResult.IsError)
        {
            logger.LogError("Token generation failed during Google registration for UserId {UserId}: {Error}", user.Id, tokensResult.TopError);

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

        // Store only the hash in the database.
        var hashedToken = tokenHasherService.HashToken(finalTokens.RefreshToken);

        var refreshTokenResult = RefreshToken.Create(
            userId: user.Id,
            value: hashedToken,
            expiresAtUtc: refreshTokenExpiresAtUtc);

        if (refreshTokenResult.IsError)
            return refreshTokenResult.TopError;

        unitOfWork.RefreshTokens.Create(refreshTokenResult.Value);

        // 9. Save changes to the database

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Google registration succeeded for UserId {UserId}", user.Id);

        return new AuthResponse(
            User: mapper.Map<UserDto>(user),
            Tokens: finalTokens);
    }
}