using FluentValidation;

namespace Application.Features.Authentications.Command.Refresh;

public sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(x => x.email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.refreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
