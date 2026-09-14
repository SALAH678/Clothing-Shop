using FluentValidation;

namespace Application.Features.Purchases.Command.CreatePurchase;

public class CreatePurchaseCommandValidator : AbstractValidator<CreatePurchaseCommand>
{
    public CreatePurchaseCommandValidator()
    {
        RuleFor(x => x.CustomerPhone)
            .NotEmpty()
            .Matches(@"^0[5-7][0-9]{8}$")
            .WithMessage("Phone number must be a valid Algerian mobile number.");

        RuleFor(x => x.Origin)
            .NotEmpty()
            .WithMessage("Origin Required.")
            .Must(x => x == "BuyNow" || x == "Cart")
            .WithMessage("Origin must be either 'BuyNow' or 'Cart'.");

        RuleFor(x => x.CustomerAddress)
            .NotNull()
            .WithMessage("Customer address is required.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item is required to create a purchase.");

        RuleForEach(x => x.Items)
            .SetValidator(new PurchaseLineItemValidator());

        RuleFor(x => x.Items)
            .Must(HaveNoDuplicateVariants)
            .WithMessage("Duplicate variants in a single purchase are not allowed; combine quantities instead.")
            .When(x => x.Items is { Count: > 0 });
    }

    private static bool HaveNoDuplicateVariants(List<PurchaseLineItem> items) =>
        items.Select(i => i.VariantId).Distinct().Count() == items.Count;
}

public class PurchaseLineItemValidator : AbstractValidator<PurchaseLineItem>
{
    public PurchaseLineItemValidator()
    {
        RuleFor(x => x.VariantId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .WithMessage("Quantity per item cannot exceed 50.");
    }
}
