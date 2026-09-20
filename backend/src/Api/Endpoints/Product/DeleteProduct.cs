using Api.Extensions;
using Application.Features.Products.Commands.DeleteProduct;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Product;

public class DeleteProduct(IMediator mediator) : Endpoint<DeleteProductCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Delete("/{productId:guid}");
        Group<ProductGroup>();
        Roles("Admin");
        Options(x => x.RequireRateLimiting("admin-write"));

        Summary(s =>
        {
            s.Summary = "Delete a product";
            s.Description = "Deletes a product by id.";
            s.ExampleRequest = new DeleteProductCommand(Guid.Parse("11111111-1111-1111-1111-111111111111"));
            s.Responses[200] = "Product deleted successfully.";
            s.Responses[400] = "Product id is invalid.";
            s.Responses[404] = "Product was not found.";
            s.Responses[500] = "Product deletion failed.";
        });

        Description(x => x
            .Produces<Deleted>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(DeleteProductCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
