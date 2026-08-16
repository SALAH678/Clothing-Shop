using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using Domain.Users.VerificationTokens;
using Domain.Users.VerificationTokens.Enum;
using MediatR;

namespace Application.Features.Authentications.Command.ForgotPassword;

public class ForgotPasswordCommandHandler(IUnitOfWork unitOfWork,
    ICodeGenerator codeGenerator, IEmailService emailService) : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICodeGenerator _codeGenerator = codeGenerator;
    private readonly IEmailService _emailService = emailService;

    public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);

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

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            return ApplicationErrors.ForgotPasswordFailed;
        }

        try
        {
            await _emailService.SendVerificationCodeAsync(user.Email.Value, code, cancellationToken);
        }
        catch
        {
            //log this 
        }

        return "A verification code has been sent to your email address. Please check your inbox and follow the instructions to reset your password.";
    }
}
