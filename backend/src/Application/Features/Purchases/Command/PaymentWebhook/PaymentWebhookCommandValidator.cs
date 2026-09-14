using FluentValidation;

namespace Application.Features.Purchases.Command.PaymentWebhook;

public class PaymentWebhookCommandValidator : AbstractValidator<PaymentWebhookCommand>
{
    public PaymentWebhookCommandValidator()
    {
        RuleFor(x => x.CheckoutId)
            .NotEmpty()
            .WithMessage("Checkout id is required.");

        RuleFor(x => x.EventType)
            .NotEmpty()
            .WithMessage("Event type is required.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required.");
    }
}
