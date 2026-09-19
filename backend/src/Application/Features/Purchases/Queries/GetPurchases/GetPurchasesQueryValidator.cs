using FluentValidation;

namespace Application.Features.Purchases.Queries.GetPurchases;

public sealed class GetPurchasesQueryValidator : AbstractValidator<GetPurchasesQuery>
{
    public GetPurchasesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than zero.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");
    }
}
