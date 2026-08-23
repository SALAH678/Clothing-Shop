using Api.Extensions;
using Application.Features.Carts.Commands.CreateCart;
using Application.Features.Carts.Dtos;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Cart;
public class CreateCart(IMediator mediator) : EndpointWithoutRequest<Result<CartDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/");
        Group<CartGroup>();

        Summary(s =>
        {
            s.Summary = "Create a cart for the current user";
            s.Description = "Creates a cart for the authenticated user if one does not already exist.";
            s.Responses[200] = "Cart created successfully.";
            s.Responses[401] = "Authentication is required.";
            s.Responses[400] = "The cart payload or request context is invalid.";
            s.Responses[409] = "A cart already exists for this user.";
            s.Responses[500] = "The cart could not be created.";
        });

        Description(x => x
            .Produces<CartDto>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(409)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateCartCommand(), ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
