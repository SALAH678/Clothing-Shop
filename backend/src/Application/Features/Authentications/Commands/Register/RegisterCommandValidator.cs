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
            .NotEmpty()
            .Matches(@"^0[5-7][0-9]{8}$")
            .WithMessage("Phone number must be a valid Algerian mobile number.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => role == "Customer" || role == "Admin").WithMessage("Role must be either 'Customer' or 'Admin'.")
            .When(x => !string.IsNullOrEmpty(x.Role));
    }
}
