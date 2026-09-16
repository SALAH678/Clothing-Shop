using FluentValidation;

namespace Application.Features.Authentications.Commands.LogInExternalAuth;

public sealed class LogInExternalAuthCommandValidator : AbstractValidator<LogInExternalAuthCommand>
{
    public LogInExternalAuthCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty()
            .WithMessage("Id token is required.");
    }
}
