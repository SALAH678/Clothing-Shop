
using Api.Extensions;
using Application.Features.Purchases.Command.RetryPurchase;
using FastEndpoints;
using MediatR;

namespace Api.Endpoints.Purchase;

public class RetryPurchase(IMediator mediator) : Endpoint<RetryPurchaseCommand, IResult>
{
    private readonly IMediator _mediator = mediator;
    public override void Configure()
    {
        Post("retry/{purchaseId:guid}");
        Group<PurchaseGroup>();
        Roles("Admin", "Customer");
        Options(x => x.RequireRateLimiting("purchase-strict"));

        Summary(s =>
        {
            s.Summary = "Retry a failed purchase payment";

            s.Description =
                "Retries the payment for an existing purchase whose previous payment attempt failed. \n" +
                "The purchase must belong to the authenticated user and its payment must have a Failed status. \n" +
                "The application checks stock availability, reserves the required stock, creates a new payment checkout, " +
                "and returns the new checkout URL. \n" +
                "The existing purchase and payment are reused; a new purchase is not created.";

            s.Responses[200] = "Payment retry initiated successfully.";
            s.Responses[400] = "Invalid request.";
            s.Responses[401] = "Authentication is required.";
            s.Responses[403] = "The purchase does not belong to the authenticated user.";
            s.Responses[404] = "The purchase or payment was not found.";
            s.Responses[409] = "The payment cannot be retried or there is insufficient stock.";
            s.Responses[500] = "Failed to initiate the payment retry.";
        });

        Description(x => x
            .Produces<RetryPurchaseResult>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(403)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(409)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(RetryPurchaseCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}