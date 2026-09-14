using FluentValidation;

namespace Application.Features.Purchases.Command.RetryPurchase;

public class RetryPurchaseCommandValidator : AbstractValidator<RetryPurchaseCommand>
{
    public RetryPurchaseCommandValidator()
    {
        RuleFor(x => x.PurchaseId)
            .NotEmpty().WithMessage("PurchaseId must not be empty.")
            .Must(BeAValidGuid).WithMessage("PurchaseId must be a valid GUID.");
    }

    private bool BeAValidGuid(Guid purchaseId) => Guid.TryParse(purchaseId.ToString(), out _);
}
