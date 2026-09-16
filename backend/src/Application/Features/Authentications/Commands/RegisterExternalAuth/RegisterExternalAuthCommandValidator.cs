using FluentValidation;

namespace Application.Features.Authentications.Commands.RegisterExternalAuth;

public sealed class RegisterExternalAuthCommandValidator : AbstractValidator<RegisterExternalAuthCommand>
{
    public RegisterExternalAuthCommandValidator()
    {
        RuleFor(x => x.IdToken)
           .NotEmpty()
           .WithMessage("Id token is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^0[5-7][0-9]{8}$")
            .WithMessage("Phone number must be a valid Algerian mobile number.");
    }
}
