using FluentValidation;

namespace Application.Features.Variants.Commands.CreateVariant;

public sealed class CreateVariantCommandValidator : AbstractValidator<CreateVariantCommand>
{
    public CreateVariantCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Size)
            .NotEmpty().WithMessage("Variant size is required.")
            .MaximumLength(50).WithMessage("Variant size must not exceed 50 characters.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Variant color is required.")
            .MaximumLength(50).WithMessage("Variant color must not exceed 50 characters.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Variant stock quantity cannot be negative.");
    }
}