using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
using Domain.Common.ValueObjects.Password;
using Domain.Users.VerificationTokens.Enum;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Authentications.Command.ResetPassword;

public sealed class ResetPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService, ILogger<ResetPasswordCommandHandler> logger) : IRequestHandler<ResetPasswordCommand, Result<string>>
{
    private readonly ILogger<ResetPasswordCommandHandler> _logger = logger;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPasswordService _passwordService = passwordService;

    public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Password reset attempt for Email: {Email}", request.Email);

        var emailResult = Email.Create(request.Email);

        var user = await _unitOfWork.Users.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (user is null)
            return ApplicationErrors.InvalidPasswordResetRequest;

        var account = await _unitOfWork.Accounts.GetByUserIdAsync(user.Id, cancellationToken);

        if (account is null)
            return ApplicationErrors.InvalidPasswordResetRequest;

        var verificationToken = await _unitOfWork.VerificationTokens.GetByUserIdAsync(user.Id, VerificationTokenType.PasswordReset, cancellationToken);

        if (verificationToken is null)
            return ApplicationErrors.VerificationTokenNotFound;

        var verificationResult = verificationToken.Verify(request.Code);

        if (!verificationResult.IsSuccess)
            return verificationResult.TopError;

        var password = Password.Create(_passwordService.HashPassword(request.NewPassword));

        if (!password.IsSuccess)
            return password.TopError;

        var markAsUsed = verificationToken.MarkAsUsed();

        if (!markAsUsed.IsSuccess)
            return markAsUsed.TopError;

        var result = account.ChangePassword(password.Value);

        if (!result.IsSuccess)
            return result.TopError;

        // Revoke existing refresh tokens here.
       var refreshTokens = await _unitOfWork.RefreshTokens.GetByUserIdAsync(user.Id, cancellationToken);
        
        foreach(var refreshToken in refreshTokens)
        {
            var revokeResult = refreshToken.Revoke();

            if (!revokeResult.IsSuccess)
                return revokeResult.TopError;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset successful for Email: {Email}, UserId: {UserId}", request.Email, user.Id);

        return "Your password has been reset successfully.";
    }
}
