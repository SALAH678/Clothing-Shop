using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.BackgroundJobs;
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
using Microsoft.Extensions.Logging;

namespace Application.Features.Authentications.Command.Register;

public class RegisterCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService,
    ICodeGenerator codeGenerator, IEmailJob emailJob, ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, Result<string>>
{
    private readonly ILogger<RegisterCommandHandler> _logger = logger;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly ICodeGenerator _codeGenerator = codeGenerator;
    private readonly IEmailJob _emailJob = emailJob;

    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registration attempt for Email: {Email}", request.Email);

        var email = Email.Create(request.Email);

        if (!email.IsSuccess)
            return email.TopError;

        var password = Password.Create(_passwordService.HashPassword(request.Password));

        if (!password.IsSuccess)
            return password.TopError;

        var phoneNumber = PhoneNumber.Create(request.PhoneNumber);

        if (!phoneNumber.IsSuccess)
            return phoneNumber.TopError;

        var existsUser = await _unitOfWork.Users.GetByEmailAsync(email.Value, cancellationToken);
        User? user;

        if (existsUser is not null)
        {
            var localAccountExists = await _unitOfWork.Accounts.GetByUserIdAsync(existsUser.Id, cancellationToken);

            if (localAccountExists is not null)
            {
                _logger.LogWarning("Registration failed: email already exists for Email: {Email}", request.Email);
                return ApplicationErrors.EmailAlreadyExists;
            }

            user = existsUser;
        }
        else
        {
            Role role = Enum.TryParse<Role>(request.Role, ignoreCase: true, out var parsedRole) ? parsedRole : Role.Customer;

            var userResult = User.Create(request.FirstName, request.LastName, email.Value, phoneNumber.Value, parsedRole);

            if (!userResult.IsSuccess)
                return userResult.TopError;

            user = userResult.Value;
            _unitOfWork.Users.Create(user);
        }         
        
        var account = Account.Create(userId: user!.Id, provider: "local", password: password.Value);

        if (!account.IsSuccess)
            return account.TopError;

        var code = _codeGenerator.GenerateCode();

        var verificationTokenResult = VerificationToken.Create(user.Id, code, DateTimeOffset.UtcNow.AddMinutes(5), VerificationTokenType.EmailVerification);

        if (!verificationTokenResult.IsSuccess)
            return verificationTokenResult.TopError;


        _unitOfWork.Accounts.Create(account.Value);
        _unitOfWork.VerificationTokens.Create(verificationTokenResult.Value);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            if (user.EmailVerified)
            {
                _logger.LogInformation("User Email: {Email} is already verified. No verification email sent.", request.Email);
                return "Registration successful. Your email is already verified.";
            }
            await _emailJob.ScheduleSendVerificationCodeAsync(request.Email, code, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration: failed to schedule verification email for Email: {Email}, UserId: {UserId}", request.Email, user.Id);
        }

        _logger.LogInformation("Registration successful for Email: {Email}, UserId: {UserId}", request.Email, user.Id);

        return "Registration successful. Please check your email to verify your account.";
    }
}
