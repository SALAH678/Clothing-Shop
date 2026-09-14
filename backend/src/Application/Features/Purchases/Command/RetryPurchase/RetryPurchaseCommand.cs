using Domain.Common.Results;
using MediatR;

namespace Application.Features.Purchases.Command.RetryPurchase;

public record RetryPurchaseCommand(Guid PurchaseId)
    : IRequest<Result<RetryPurchaseResult>>;

public record RetryPurchaseResult(string CheckoutUrl);
