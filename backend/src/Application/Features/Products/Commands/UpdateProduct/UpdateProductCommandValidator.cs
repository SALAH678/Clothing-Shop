using FluentValidation;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MinimumLength(3).WithMessage("Product name must be at least 3 characters long.")
            .MaximumLength(150).WithMessage("Product name must not exceed 150 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description is required.")
            .MinimumLength(10).WithMessage("Product description must be at least 10 characters long.")
            .MaximumLength(2000).WithMessage("Product description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("Base price must be greater than zero.")
            .When(x => x.BasePrice.HasValue);

        RuleFor(x => x.Discount)
            .InclusiveBetween(0m, 100m)
            .WithMessage("Discount must be between 0 and 100.")
            .When(x => x.Discount.HasValue);
    }
}
