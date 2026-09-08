using Api.Extensions;
using Application.Features.Carts.Commands.RemoveCartItem;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Cart;
public class RemoveCartItem(IMediator mediator) : Endpoint<RemoveCartItemCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Delete("/items/{CartItemId:guid}");
        Group<CartGroup>();
        Roles("Admin", "Customer");

        Summary(s =>
        {
            s.Summary = "Remove an item from the cart";
            s.Description = "Deletes a specific cart item for the current user.";
            s.ExampleRequest = new RemoveCartItemCommand(Guid.Parse("11111111-1111-1111-1111-111111111111"));
            s.Responses[200] = "The cart item was removed successfully.";
            s.Responses[401] = "Authentication is required.";
            s.Responses[400] = "The cart item identifier is invalid.";
            s.Responses[404] = "The cart or cart item was not found.";
            s.Responses[500] = "The cart item could not be removed.";
        });

        Description(x => x
            .Produces<Deleted>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(RemoveCartItemCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
