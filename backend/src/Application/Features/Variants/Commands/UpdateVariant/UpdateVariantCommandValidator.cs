using FluentValidation;

namespace Application.Features.Variants.Commands.UpdateVariant;

public sealed class UpdateVariantCommandValidator : AbstractValidator<UpdateVariantCommand>
{
    public UpdateVariantCommandValidator()
    {
        RuleFor(x => x.VariantId)
            .NotEmpty().WithMessage("Variant ID is required.");

        RuleFor(x => x.Size)
            .NotEmpty().WithMessage("Variant size is required.")
            .MaximumLength(50).WithMessage("Variant size must not exceed 50 characters.")
            .When(x => x.Size is not null);

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Variant color is required.")
            .MaximumLength(50).WithMessage("Variant color must not exceed 50 characters.")
            .When(x => x.Color is not null);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Variant stock quantity cannot be negative.")
            .When(x => x.StockQuantity.HasValue);

        RuleFor(x => x)
            .Must(x => x.Size is not null || x.Color is not null || x.StockQuantity.HasValue)
            .WithMessage("At least one variant field must be provided.");
    }
}