using FluentValidation;

namespace Application.Features.Authentications.Command.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6);

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8);
    }
}
