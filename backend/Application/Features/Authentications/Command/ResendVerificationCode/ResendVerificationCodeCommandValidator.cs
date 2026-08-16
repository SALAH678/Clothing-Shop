using FluentValidation;

namespace Application.Features.Authentications.Command.ResendVerificationCode;

public sealed class ResendVerificationCodeCommandValidator : AbstractValidator<ResendVerificationCodeCommand>
{
    public ResendVerificationCodeCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Invalid email address.");
    }
}
