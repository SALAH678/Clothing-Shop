using FluentValidation;
using System.Text.RegularExpressions;

namespace Application.Features.Authentications.Command.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private static readonly string nameRegex = @"^[a-zA-Z\s]+$";
    private static readonly Regex PasswordRegex = new(
       @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{6,}$",
       RegexOptions.Compiled

    );

    private static readonly Regex PhoneRegex = new(@"^\d{10}$", RegexOptions.Compiled);

    public RegisterCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .Matches(nameRegex).WithMessage("First name cannot contain special characters or numbers.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Matches(nameRegex).WithMessage("Last name cannot contain special characters or numbers.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Matches(PasswordRegex).WithMessage("Password must be at least 6 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(PhoneRegex).WithMessage("Phone number must be exactly 10 digits long.");
    }
}
