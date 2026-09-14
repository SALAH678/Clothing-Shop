using Api.Extensions;
using Application.Features.Purchases.Command.PaymentWebhook;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Purchase;

public class PaymentWebhook(IMediator mediator) : Endpoint<PaymentWebhookCommand, IResult>
{
    private readonly IMediator _mediator = mediator;
    public override void Configure()
    {
        Post("webhook/payment");
        Group<PurchaseGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Handle payment gateway webhook";

            s.Description =
                "Receives payment status notifications from the payment gateway. \n" +
                "The webhook is used to update the payment associated with the checkout. \n" +
                "The application processes terminal payment states such as paid, failed, expired, " +
                "and canceled, while non-terminal states are ignored. \n" +
                "The webhook signature must be validated before processing the request.";

            s.ExampleRequest = new PaymentWebhookCommand(
                CheckoutId: "01JABCDEF123456789XYZ",
                EventType: "checkout.paid",
                Status: "paid"
            );

            s.Responses[200] = "Webhook processed successfully.";
            s.Responses[400] = "Invalid webhook request or signature.";
            s.Responses[404] = "The payment associated with the checkout was not found.";
            s.Responses[500] = "Failed to process the webhook.";
        });

        Description(x => x
            .Produces<Updated>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(PaymentWebhookCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
