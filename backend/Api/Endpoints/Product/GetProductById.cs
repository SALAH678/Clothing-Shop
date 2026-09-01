using Api.Extensions;
using Application.Features.Products.Dtos;
using Application.Features.Products.Queries.GetProductById;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Product;

public class GetProductById(IMediator mediator) : Endpoint<GetProductByIdQuery, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/{productId:guid}");
        Group<ProductGroup>();
        Roles("Admin", "Customer");

        Summary(s =>
        {
            s.Summary = "Get product by id";
            s.Description = "Returns a specific product by id.";
            s.ExampleRequest = new GetProductByIdQuery(Guid.Parse("11111111-1111-1111-1111-111111111111"));
            s.Responses[200] = "Product retrieved successfully.";
            s.Responses[400] = "Product id is invalid.";
            s.Responses[404] = "Product was not found.";
        });

        Description(x => x
            .Produces<ProductDto>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404));
    }

    public override async Task<IResult> ExecuteAsync(GetProductByIdQuery req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
