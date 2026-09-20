using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.BackgroundJobs;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
using Domain.Users.VerificationTokens;
using Domain.Users.VerificationTokens.Enum;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Authentications.Command.ResendVerificationCode;

public class ResendCodeCommandHandler(IUnitOfWork unitOfWork, ICodeGenerator codeGenerator,
    IEmailJob emailJob, ILogger<ResendCodeCommandHandler> logger) : IRequestHandler<ResendCodeCommand, Result<string>>
{
    private readonly ILogger<ResendCodeCommandHandler> _logger = logger;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICodeGenerator _codeGenerator = codeGenerator;
    private readonly IEmailJob _emailJob = emailJob;

    //i think u need to use strategy pattern here
    public async Task<Result<string>> Handle(ResendCodeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resend code requested for Email: {Email}, Type: {Type}", request.Email, request.VerificationTokenType);

        var emailResult = Email.Create(request.Email);

        var user = await _unitOfWork.Users.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (user is null)
            return ApplicationErrors.InvalidVerificationRequest;

        if(Enum.TryParse<VerificationTokenType>(request.VerificationTokenType, out var verificationTokenType) is false)
            return ApplicationErrors.InvalidVerificationRequest;

        switch (verificationTokenType)
        {
            case VerificationTokenType.EmailVerification:

                if (user.EmailVerified)
                    return ApplicationErrors.EmailAlreadyVerified;

                break;

            case VerificationTokenType.PasswordReset:

                var account = await _unitOfWork.Accounts
                    .GetByUserIdAsync(user.Id, cancellationToken);

                if (account is null)
                    return ApplicationErrors.InvalidVerificationRequest;

                break;

            default:
                return ApplicationErrors.InvalidVerificationRequest;
        }

        var verificationToken = await _unitOfWork.VerificationTokens.GetByUserIdAsync(user.Id, verificationTokenType, cancellationToken);

        if (verificationToken is not null)
            _unitOfWork.VerificationTokens.Delete(verificationToken);

        var code = _codeGenerator.GenerateCode();

        var verificationTokenResult = VerificationToken.Create(user.Id, code, DateTimeOffset.UtcNow.AddMinutes(5), verificationTokenType);

        if (!verificationTokenResult.IsSuccess)
            return verificationTokenResult.TopError;

        _unitOfWork.VerificationTokens.Create(verificationTokenResult.Value);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            if (verificationTokenType == VerificationTokenType.EmailVerification)
                await _emailJob.ScheduleSendVerificationCodeAsync(user.Email.Value, code, cancellationToken);
            else
                await _emailJob.ScheduleSendPasswordResetCodeAsync(user.Email.Value, code, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResendCode: failed to schedule email job for Email: {Email}, UserId: {UserId}, Type: {Type}", request.Email, user.Id, verificationTokenType);
        }

        return "A new code has been sent.";
    }
}
