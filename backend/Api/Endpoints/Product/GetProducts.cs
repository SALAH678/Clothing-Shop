using Api.Extensions;
using Application.Common.Models;
using Application.Features.Products.Dtos;
using Application.Features.Products.Queries.GetProducts;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Product;

public class GetProducts(IMediator mediator) : Endpoint<GetProductsQuery, Result<PaginatedList<ProductDto>>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/");
        Group<ProductGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get paginated products";
            s.Description = "Returns paginated products filtered by category and optional filter criteria.";
            s.ExampleRequest = new GetProductsQuery(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                1,
                10,
                new ProductFilter(
                    "shirt",
                    10,
                    200,
                    "M",
                    "Black",
                    "price",
                    false));
            s.Responses[200] = "Products retrieved successfully.";
            s.Responses[400] = "Product query parameters are invalid.";
        });

        Description(x => x
            .Produces<PaginatedList<ProductDto>>(200)
            .ProducesProblemDetails(400));
    }

    public override async Task<IResult> HandleAsync(GetProductsQuery req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
