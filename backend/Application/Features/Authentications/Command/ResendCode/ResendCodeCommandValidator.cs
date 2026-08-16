using FluentValidation;

namespace Application.Features.Authentications.Command.ResendVerificationCode;

public sealed class ResendCodeCommandValidator : AbstractValidator<ResendCodeCommand>
{
    public ResendCodeCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Invalid email address.");

        RuleFor(x => x.VerificationTokenType)
            .NotEmpty()
            .WithMessage("Verification token type is required.")
            .Matches("^(PasswordReset|EmailVerification)$")
            .WithMessage("type must be either 'PasswordReset' or 'EmailVerification'.");
    }
}
