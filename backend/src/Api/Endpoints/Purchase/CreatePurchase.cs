using Api.Extensions;
using Application.Features.Purchases.Command.CreatePurchase;
using Domain.Common.ValueObjects.Address;
using FastEndpoints;
using MediatR;
//using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Purchase;

public class CreatePurchase(IMediator mediator) : Endpoint<CreatePurchaseRequest, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("");
        Group<PurchaseGroup>();
        Roles("Admin", "Customer");

        Summary(s =>
        {
            s.Summary = "Create a purchase";
            s.Description =
                "Creates a purchase for the authenticated customer and initiates the payment process. \n" +
                "The purchase must contain at least one item. Each item specifies a product variant " +
                "and the quantity to purchase. \n" +
                "The customer address must include Street, City, and Wilaya. \n" +
                "Each VariantId should appear only once in the PurchaseItems array. \n" +
                "The unit price is determined by the server using the current product price and discount. \n" +
                "If the purchase is created successfully, a payment checkout URL is returned.";

            s.ExampleRequest = new CreatePurchaseRequest
            {
                CustomerPhone = "0550123456",
                Street = "12 Rue Didouche Mourad",
                City = "Algiers",
                Wilaya = "Algiers",
                Origin = "BuyNow | Cart",
                PurchaseItems = new List<PurchaseItem>
        {
            new()
            {
                VariantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Quantity = 2
            },
            new()
            {
                VariantId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Quantity = 1
            }
        }
            };

            s.Responses[200] = "Purchase created successfully and payment checkout initiated.";
            s.Responses[400] = "The purchase request or customer address is invalid.";
            s.Responses[401] = "Authentication is required.";
            s.Responses[409] = "One or more requested variants do not have sufficient stock.";
            s.Responses[500] = "Purchase creation or payment initiation failed.";
        });

        Description(x => x
            .Produces<CreatePurchaseResult>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(409)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(CreatePurchaseRequest req, CancellationToken ct)
    {
        var customerAddress = Address.Create(req.Street, req.City, req.Wilaya);
        if (customerAddress.IsError)
            return Results.BadRequest(new
            {
                error = customerAddress.TopError.Description
            });

        var items = req.PurchaseItems
            .Select(x => new PurchaseLineItem(
                 x.VariantId,
                 x.Quantity))
            .ToList();

        var command = new CreatePurchaseCommand(req.CustomerPhone, req.Origin, customerAddress.Value, items);

        var result = await _mediator.Send(command, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}

public sealed class CreatePurchaseRequest
{
    public string CustomerPhone { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Wilaya { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public List<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}

public sealed class PurchaseItem
{
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
}