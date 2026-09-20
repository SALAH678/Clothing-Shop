using Api.Extensions;
using Application.Features.Carts.Commands.AddCartItem;
using Application.Features.Carts.Dtos;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Cart;
public class AddCartItem(IMediator mediator) : Endpoint<AddCartItemCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/items/{variantId:guid}");
        Group<CartGroup>();
        Roles("Admin", "Customer");
        Options(x => x.RequireRateLimiting("cart-write"));

        Summary(s =>
        {
            s.Summary = "Add an item to the cart";
            s.Description = "Adds a product variant to the current user's cart or increases its quantity.";
            s.ExampleRequest = new AddCartItemCommand(Guid.Parse("44444444-4444-4444-4444-444444444444"), 2);
            s.Responses[200] = "The cart was updated successfully.";
            s.Responses[401] = "Authentication is required.";
            s.Responses[400] = "The cart item payload is invalid.";
            s.Responses[404] = "The cart for the current user was not found.";
            s.Responses[500] = "The cart item could not be added.";
        });

        Description(x => x
            .Produces<CartDto>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(AddCartItemCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
