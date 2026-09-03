using FluentValidation;

namespace Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than zero.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");

        RuleFor(x => x.Filter)
            .NotNull().WithMessage("Product filter is required.");

        When(x => x.Filter is not null, () =>
        {
            RuleFor(x => x.Filter.Search)
                .MaximumLength(100).WithMessage("Search text must not exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Filter.Search));

            RuleFor(x => x.Filter.MinPrice)
                .GreaterThanOrEqualTo(0m).WithMessage("Minimum price cannot be negative.")
                .When(x => x.Filter.MinPrice.HasValue);

            RuleFor(x => x.Filter.MaxPrice)
                .GreaterThanOrEqualTo(0m).WithMessage("Maximum price cannot be negative.")
                .When(x => x.Filter.MaxPrice.HasValue);

            RuleFor(x => x)
                .Must(x => !x.Filter.MinPrice.HasValue || !x.Filter.MaxPrice.HasValue || x.Filter.MinPrice <= x.Filter.MaxPrice)
                .WithMessage("Minimum price cannot be greater than maximum price.");

            RuleFor(x => x.Filter.Sizes)
                .Must(sizes => sizes!.Count <= 20)
                .WithMessage("You can filter by at most 20 sizes at once.")
                .When(x => x.Filter.Sizes is { Count: > 0 });

            RuleForEach(x => x.Filter.Sizes)
                .NotEmpty().WithMessage("Size values cannot be empty.")
                .MaximumLength(50).WithMessage("Size must not exceed 50 characters.")
                .When(x => x.Filter.Sizes is { Count: > 0 });

            RuleFor(x => x.Filter.Colors)
                .Must(colors => colors!.Count <= 20)
                .WithMessage("You can filter by at most 20 colors at once.")
                .When(x => x.Filter.Colors is { Count: > 0 });

            RuleForEach(x => x.Filter.Colors)
                .NotEmpty().WithMessage("Color values cannot be empty.")
                .MaximumLength(50).WithMessage("Color must not exceed 50 characters.")
                .When(x => x.Filter.Colors is { Count: > 0 });

            RuleFor(x => x.Filter.SortBy)
                .Must(sortBy => string.IsNullOrWhiteSpace(sortBy)
                    || sortBy.Equals("price", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Sort by must be 'price'.")
                .When(x => !string.IsNullOrWhiteSpace(x.Filter.SortBy));
        });
    }
}
