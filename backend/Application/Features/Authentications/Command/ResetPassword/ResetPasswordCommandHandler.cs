using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Password;
using Domain.Users.VerificationTokens.Enum;
using MediatR;

namespace Application.Features.Authentications.Command.ResetPassword;

public sealed class ResetPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService) : IRequestHandler<ResetPasswordCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPasswordService _passwordService = passwordService;

    public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);

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

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            return ApplicationErrors.PasswordResetFailed;
        }

        return "Your password has been reset successfully.";
    }
}
