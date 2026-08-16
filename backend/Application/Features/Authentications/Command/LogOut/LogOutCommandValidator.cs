using FluentValidation;

namespace Application.Features.Authentications.Command.LogOut;

public sealed class LogOutCommandValidator : AbstractValidator<LogOutCommand>
{
    public LogOutCommandValidator()
    {
        RuleFor(x => x.email)
           .NotEmpty().WithMessage("Email is required.")
           .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.refreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
