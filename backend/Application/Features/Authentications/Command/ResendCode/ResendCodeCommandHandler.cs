using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using Domain.Users.VerificationTokens;
using Domain.Users.VerificationTokens.Enum;
using MediatR;

namespace Application.Features.Authentications.Command.ResendVerificationCode;

public class ResendCodeCommandHandler(IUnitOfWork unitOfWork,
    ICodeGenerator codeGenerator, IEmailService emailService) : IRequestHandler<ResendCodeCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICodeGenerator _codeGenerator = codeGenerator;
    private readonly IEmailService _emailService = emailService;
    //i think u need to use strategy pattern here
    public async Task<Result<string>> Handle(ResendCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);

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

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            return ApplicationErrors.ResendVerificationCodeFailed;
        }

        try
        {
            await _emailService.SendVerificationCodeAsync(user.Email.Value, code, cancellationToken);
        }
        catch
        {
            //log this 
        }

        return "A new verification code has been sent.";
    }
}
