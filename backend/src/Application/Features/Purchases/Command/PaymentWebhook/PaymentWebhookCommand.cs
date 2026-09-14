using Domain.Common.Results;
using MediatR;

namespace Application.Features.Purchases.Command.PaymentWebhook;

public record PaymentWebhookCommand(
    string CheckoutId,
    string EventType,
    string Status
) : IRequest<Result<Updated>>;
