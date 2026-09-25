using Application.Common.Attributes;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Address;
using MediatR;

namespace Application.Features.Purchases.Command.CreatePurchase;

public record CreatePurchaseCommand(
    string CustomerPhone,
    string Origin,
    [property: Sensitive] Address CustomerAddress,
    List<PurchaseLineItem> Items
) : IRequest<Result<CreatePurchaseResult>>;

public record PurchaseLineItem(Guid VariantId, int Quantity);

public record CreatePurchaseResult(Guid PurchaseId, string CheckoutUrl);