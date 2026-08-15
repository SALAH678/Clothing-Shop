using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
using Domain.Common.ValueObjects.Password;
using Domain.Common.ValueObjects.PhoneNumber;
using Domain.Users;
using Domain.Users.Accounts;
using Domain.Users.Enum;
using Domain.Users.VerificationTokens;
using Domain.Users.VerificationTokens.Enum;
using MediatR;

namespace Application.Features.Authentications.Command.Register;

public class RegisterCommandHandler(
        IUnitOfWork unitOfWork, IPasswordService passwordService,
        ICodeGenerator codeGenerator, IEmailService emailService) : IRequestHandler<RegisterCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly ICodeGenerator _codeGenerator = codeGenerator;
    private readonly IEmailService _emailService = emailService;

    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        if (!email.IsSuccess)
            return email.TopError;

        var password = Password.Create(_passwordService.HashPassword(request.Password));

        if (!password.IsSuccess)
            return password.TopError;

        var phoneNumber = PhoneNumber.Create(request.PhoneNumber);

        if (!phoneNumber.IsSuccess)
            return phoneNumber.TopError;

        var emailExists = await _unitOfWork.Users.ExistsAsync(email.Value, cancellationToken);

        if (emailExists)
            return ApplicationErrors.EmailAlreadyExists;

        var user = User.Create(request.FirstName, request.LastName, email.Value, phoneNumber.Value, Role.Customer);

        if (!user.IsSuccess)
            return user.TopError;

        var account = Account.Create(userId: user.Value.Id, provider: "local", password: password.Value);

        if (!account.IsSuccess)
            return account.TopError;

        var code = _codeGenerator.GenerateCode();

        var verificationTokenResult = VerificationToken.Create(user.Value.Id, code, DateTimeOffset.UtcNow.AddMinutes(5), VerificationTokenType.EmailVerification);

        if (!verificationTokenResult.IsSuccess)
            return verificationTokenResult.TopError;

        _unitOfWork.Users.Create(user.Value);
        _unitOfWork.Accounts.Create(account.Value);
        _unitOfWork.VerificationTokens.Create(verificationTokenResult.Value);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            return ApplicationErrors.RegistrationFailed;
        }

        try
        {
            await _emailService.SendVerificationCodeAsync(request.Email, code, cancellationToken);
        }
        catch
        {
            //log this 
            //return "Account created, but verification email could not be sent. Please request another verification email.";
        }

        return "Registration successful. Please check your email to verify your account.";
    }
}
