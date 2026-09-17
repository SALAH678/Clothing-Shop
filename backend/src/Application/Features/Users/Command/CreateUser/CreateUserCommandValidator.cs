using FluentValidation;

namespace Application.Features.Users.Command.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private const string NameRegex = "^[a-zA-Z\\s]+$";

    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .Matches(NameRegex).WithMessage("First name cannot contain special characters or numbers.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Matches(NameRegex).WithMessage("Last name cannot contain special characters or numbers.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^0[5-7][0-9]{8}$").WithMessage("Phone number must be a valid Algerian mobile number.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => role == "Customer" || role == "Admin")
            .WithMessage("Role must be either 'Customer' or 'Admin'.")
            .When(x => !string.IsNullOrWhiteSpace(x.Role));
    }
}
