
using FluentValidation;
using System.Text.RegularExpressions;

namespace Application.Features.Authentications.Command.ExternalAuthentication
{
    public sealed class ExternalAuthValidator : AbstractValidator<ExternalAuthCommand>
    {
        private static readonly Regex PhoneRegex = new(@"^\d{10}$", RegexOptions.Compiled);
        public ExternalAuthValidator()
        {
            RuleFor(x => x.IdToken)
            .NotEmpty()
            .WithMessage("Id token is required.");

            RuleFor(x => x.phoneNumber)
            .Matches(PhoneRegex)
            .WithMessage("Phone number must be exactly 10 digits long.")
            .When(x => !string.IsNullOrWhiteSpace(x.phoneNumber));
        }
    }
}
