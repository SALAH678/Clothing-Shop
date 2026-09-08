using Application.Common.Errors;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
using Domain.Common.ValueObjects.PhoneNumber;
using Domain.Users;
using Domain.Users.Accounts;
using Microsoft.Extensions.Logging;

namespace Application.Common.Services;

public class ExternalAuthenticator(IOAuthService oAuthService,IUnitOfWork unitOfWork, ILogger<ExternalAuthenticator> logger) : IExternalAuthenticator
{
    public async Task<Result<User>> AuthenticateAsync(string idToken, string? phoneNumber, CancellationToken cancellationToken) 
    {
        OAuthUserInfo googleInfo;

        try
        {
            googleInfo = await oAuthService.ValidateGoogleTokenAsync(idToken, cancellationToken);
        }
        catch (InvalidOAuthTokenException ex)
        {
            logger.LogWarning(ex, "Google ID token validation failed");

            return ApplicationErrors.InvalidGoogleId;
        }

        if (!googleInfo.EmailVerified)
        {
            logger.LogWarning("OAuth login rejected - unverified Google email {Email}", googleInfo.Email);

            return ApplicationErrors.EmailNotVerified;
        }

        var account = await unitOfWork.Accounts.GetByProviderAsync(googleInfo.ProviderAccountId, cancellationToken);

        if (account is not null)
        {
            return await GetExistingUserAsync(account, cancellationToken);
        }

        return await CreateOrLinkUserAsync(googleInfo, phoneNumber, cancellationToken);
    }

    private async Task<Result<User>> GetExistingUserAsync(Account account, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdAsync(account.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogError("Orphaned account detected - AccountId {AccountId} references UserId {UserId}", account.Id, account.UserId);

            return ApplicationErrors.UserNotFound;
        }

        return user;
    }

    private async Task<Result<User>> CreateOrLinkUserAsync(OAuthUserInfo googleInfo, string? phoneNumber, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(googleInfo.Email);

        if (emailResult.IsError)
            return emailResult.TopError;

        var user = await unitOfWork.Users.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (user is null)
        {
            var phoneResult = PhoneNumber.Create(phoneNumber);

            if (phoneResult.IsError)
                return phoneResult.TopError;

            var userResult = User.Create(googleInfo.FirstName, googleInfo.LastName, emailResult.Value, phoneResult.Value);

            if (userResult.IsError)
                return userResult.TopError;

            user = userResult.Value;

            user.MarkEmailVerified();

            unitOfWork.Users.Create(user);
        }

        var accountResult = Account.Create(user.Id, "Google", googleInfo.ProviderAccountId);

        if (accountResult.IsError)
            return accountResult.TopError;

        unitOfWork.Accounts.Create(accountResult.Value);

        return user;
    }
}
