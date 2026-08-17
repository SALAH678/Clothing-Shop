using FluentValidation;

namespace Application.Features.Users.Command.UpdateCurrentUserProfile;

public sealed class UpdateCurrentUserProfileCommandValidator : AbstractValidator<UpdateCurrentUserProfileCommand>
{
    public UpdateCurrentUserProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name cannot be empty.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.")
            .Matches(@"^[\p{L}\s-]+$").WithMessage("First name can only contain letters, spaces, or hyphens.")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name cannot be empty.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.")
            .Matches(@"^[\p{L}\s-]+$").WithMessage("Last name can only contain letters, spaces, or hyphens.")
            .When(x => x.LastName is not null);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^[0-9]{10}$")
            .When(x => x.PhoneNumber is not null);
    }
}
