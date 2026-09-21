using Api.Extensions;
using Application.Features.Carts.Dtos;
using Application.Features.Carts.Queries.GetCart;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Cart;
public class GetCart(IMediator mediator) : EndpointWithoutRequest<IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("");
        Group<CartGroup>();
        Roles("Admin", "Customer");
        Options(x => x.RequireRateLimiting("authenticated-read"));

        Summary(s =>
        {
            s.Summary = "Get the current user's cart";
            s.Description = "Returns the current user's cart and all items in it.";
            s.Responses[200] = "Cart retrieved successfully.";
            s.Responses[401] = "Authentication is required.";
            s.Responses[400] = "No cart is available for the current user.";
            s.Responses[500] = "The cart could not be retrieved.";
        });

        Description(x => x
            .Produces<CartDto>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(500));
    }
    public override async Task<IResult> ExecuteAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCartQuery(), ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
