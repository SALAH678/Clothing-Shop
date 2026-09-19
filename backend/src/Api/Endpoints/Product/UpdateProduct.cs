using Api.Extensions;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.Dtos;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Product;

public class UpdateProduct(IMediator mediator) : Endpoint<UpdateProductCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/{productId:guid}");
        Group<ProductGroup>();
        Roles("Admin");

        Summary(s =>
        {
            s.Summary = "Update a product";
            s.Description = "Updates mutable product fields such as name, description, base price, and discount.";
            s.ExampleRequest = new UpdateProductCommand(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "Updated T-Shirt",
                "Updated description for the product",
                59.99m,
                10m,
                Guid.Parse("11111111-1111-1111-1111-111111111111"));
            s.Responses[200] = "Product updated successfully.";
            s.Responses[400] = "Product update payload is invalid.";
            s.Responses[500] = "Product update failed.";
        });

        Description(x => x
            .Produces<ProductDto>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(UpdateProductCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
