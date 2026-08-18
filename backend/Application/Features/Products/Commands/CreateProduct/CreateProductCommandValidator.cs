using FluentValidation;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MinimumLength(3).WithMessage("Product name must be at least 3 characters long.")
            .MaximumLength(150).WithMessage("Product name must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description is required.")
            .MinimumLength(10).WithMessage("Product description must be at least 10 characters long.")
            .MaximumLength(2000).WithMessage("Product description must not exceed 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("Base price must be greater than zero.");

        RuleFor(x => x.Discount)
            .InclusiveBetween(0m, 100m)
            .WithMessage("Discount must be between 0 and 100.")
            .When(x => x.Discount.HasValue);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.Variants)
            .Must(variants => variants is null || variants.Count > 0)
            .WithMessage("At least one variant is required when variants are supplied.")
            .When(x => x.Variants is not null);

        When(x => x.Variants is not null, () =>
        {
            RuleForEach(x => x.Variants!)
                .ChildRules(variant =>
                {
                    variant.RuleFor(v => v.Size)
                        .NotEmpty().WithMessage("Variant size is required.");

                    variant.RuleFor(v => v.Color)
                        .NotEmpty().WithMessage("Variant color is required.");

                    variant.RuleFor(v => v.StockQuantity)
                        .GreaterThanOrEqualTo(0).WithMessage("Variant stock quantity cannot be negative.");
                });
        });
    }
}
