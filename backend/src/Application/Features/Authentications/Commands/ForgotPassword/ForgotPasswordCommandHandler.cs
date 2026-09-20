using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
using Domain.Users.VerificationTokens;
using Domain.Users.VerificationTokens.Enum;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Authentications.Command.ForgotPassword;

public class ForgotPasswordCommandHandler(IUnitOfWork unitOfWork,
    ICodeGenerator codeGenerator, IEmailJob emailJob, ILogger<ForgotPasswordCommandHandler> logger) : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    private readonly ILogger<ForgotPasswordCommandHandler> _logger = logger;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICodeGenerator _codeGenerator = codeGenerator;
    private readonly IEmailJob _emailJob = emailJob;

    public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Password reset requested for Email: {Email}", request.Email);

        var emailResult = Email.Create(request.Email);

        var user = await _unitOfWork.Users.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (user is null)
            return "If an account exists with this email, a password reset code has been sent.";

        var account = await _unitOfWork.Accounts.GetByUserIdAsync(user.Id, cancellationToken);

        if (account is null)
            return "If an account exists with this email, a password reset code has been sent.";

        var code = _codeGenerator.GenerateCode();

        var verificationTokenResult = VerificationToken.Create(user.Id, code, DateTimeOffset.UtcNow.AddMinutes(5), VerificationTokenType.PasswordReset);

        if (!verificationTokenResult.IsSuccess)
            return verificationTokenResult.TopError;

        _unitOfWork.VerificationTokens.Create(verificationTokenResult.Value);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailJob.ScheduleSendPasswordResetCodeAsync(user.Email.Value, code, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ForgotPassword: failed to schedule password reset email for Email: {Email}, UserId: {UserId}", request.Email, user.Id);
        }

        return "A verification code has been sent to your email address. Please check your inbox and follow the instructions to reset your password.";
    }
}
