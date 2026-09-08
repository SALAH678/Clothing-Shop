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

namespace Application.Features.Authentications.Command.ExternalAuthentication;

public class ExternalAuthHandler(IOAuthService oAuthService, IUnitOfWork unitOfWork, ITokenProvider tokenProvider,
    IMapper mapper, ITokenHasherService tokenHasherService, ILogger<ExternalAuthHandler> logger) : IRequestHandler<ExternalAuthCommand, Result<AuthResponse>>
{
    private readonly IOAuthService _oAuthService = oAuthService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenHasherService _tokenHasherService = tokenHasherService;
    private readonly ILogger<ExternalAuthHandler> _logger = logger;

    public async Task<Result<AuthResponse>> Handle(ExternalAuthCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("OAuth login attempt started for provider {Provider}", "Google");

        OAuthUserInfo googleInfo;

        try
        {
            googleInfo = await _oAuthService.ValidateGoogleTokenAsync(request.IdToken, cancellationToken);
        }
        catch (InvalidOAuthTokenException ex)
        {
            _logger.LogWarning(ex, "Google ID token validation failed");
            return ApplicationErrors.InvalidGoogleId;
        }

        if (!googleInfo.EmailVerified)
        {
            _logger.LogWarning("OAuth login rejected - unverified Google email {Email}", googleInfo.Email);
            return ApplicationErrors.EmailNotVerified;
        }
            
        // 2. Check if an Account already exists for this provider + providerAccountId -> LOGIN path
        var existingAccount = await _unitOfWork.Accounts
                .GetByProviderAsync(googleInfo.ProviderAccountId, cancellationToken);

        User? user;

        if (existingAccount is not null)
        {
            _logger.LogInformation("Existing Google account found for ProviderAccountId {ProviderAccountId} - proceeding with login",
                googleInfo.ProviderAccountId);

            user = await _unitOfWork.Users.GetByIdAsync(existingAccount.UserId, cancellationToken);

            if (user is null)
            {
                _logger.LogError("Orphaned account detected - AccountId {AccountId} references missing UserId {UserId}",
                    existingAccount.Id, existingAccount.UserId);

                return ApplicationErrors.UserNotFound;
            }
        }
        else
        {
            // 3. No account yet -> REGISTER path (with account-linking by email if a local user exists)
            var emailResult = Email.Create(googleInfo.Email);
            if (emailResult.IsError)
            {
                _logger.LogWarning("Email creation failed for {Email}: {Error}", googleInfo.Email, emailResult.TopError);
                return emailResult.TopError;
            }
                
            user = await _unitOfWork.Users.GetByEmailAsync(emailResult.Value, cancellationToken);

            if (user is null)
            {
                var phoneNumberResult = PhoneNumber.Create(request.phoneNumber);
                if (phoneNumberResult.IsError)
                {
                    _logger.LogWarning("Phone number creation failed for {Email}: {Error}", googleInfo.Email, phoneNumberResult.TopError);
                    return phoneNumberResult.TopError;
                }

                _logger.LogInformation("No existing user for email {Email} - creating new user via Google OAuth", googleInfo.Email);

                var userResult = User.Create(googleInfo.FirstName, googleInfo.LastName, emailResult.Value, phoneNumberResult.Value);
                if(userResult.IsError)
                {
                    _logger.LogWarning("User creation failed for {Email}: {Error}", googleInfo.Email, userResult.TopError);
                    return userResult.TopError;
                }

                user = userResult.Value;

                _logger.LogInformation("Created new user {UserId} ({Email}) via Google OAuth", user.Id, googleInfo.Email);

                _unitOfWork.Users.Create(user);
            }

            var newAccount = Account.Create(user.Id, "Google", googleInfo.ProviderAccountId);
            if(newAccount.IsError)
            {
                _logger.LogWarning("Account creation failed for UserId {UserId}: {Error}", user.Id, newAccount.TopError);
                return newAccount.TopError;
            }

            _unitOfWork.Accounts.Create(newAccount.Value);
        }

        // 4. Issue tokens - same path as local login
        var tokens = _tokenProvider.GenerateJwtToken(user.Id.ToString(), user.Email.Value, user.UserRole.ToString());

        if (tokens.IsError)
        {
            _logger.LogError("Token generation failed for UserId {UserId}: {Error}", user.Id, tokens.TopError);
            return tokens.TopError;
        }


        var refreshTokenExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(7);
        var finalTokens = tokens.Value with { RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc };

        if (finalTokens.RefreshToken is null)
        {
            _logger.LogError( "Login failed: generated refresh token is null for UserId: {UserId}", user.Id);

            return ApplicationErrors.LoginFailed;
        }

        var hashedToken = _tokenHasherService.HashToken(finalTokens.RefreshToken);

        var refreshToken = RefreshToken.Create(
            userId: user.Id,
            value: hashedToken,
            expiresAtUtc: refreshTokenExpiresAtUtc);

        if (refreshToken.IsError)
        {
            _logger.LogError("Refresh token creation failed for UserId {UserId}: {Error}", user.Id, refreshToken.TopError);
            return refreshToken.TopError;
        }

        _unitOfWork.RefreshTokens.Create(refreshToken.Value);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving changes in OAuth login");
            return ApplicationErrors.OAuthFailed;
        }

        _logger.LogInformation("OAuth login succeeded for UserId {UserId}", user.Id);

        return new AuthResponse(
        User: _mapper.Map<UserDto>(user),
        Tokens: finalTokens);
    }
}
