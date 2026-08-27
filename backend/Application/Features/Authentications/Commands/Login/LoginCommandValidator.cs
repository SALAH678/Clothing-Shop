using FluentValidation;
using System.Text.RegularExpressions;

namespace Application.Features.Authentications.Command.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    private static readonly Regex PasswordRegex = new(
      @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{6,}$",
      RegexOptions.Compiled
    );

    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Matches(PasswordRegex).WithMessage("Password must be at least 6 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
    }
}
