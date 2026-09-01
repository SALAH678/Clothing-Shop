using Api.Extensions;
using Application.Common.Models;
using Application.Features.Products.Dtos;
using Application.Features.Products.Queries.GetProducts;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Product;

public class GetProducts(IMediator mediator) : Endpoint<GetProductsRequest, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("");
        Group<ProductGroup>();
        Roles("Admin", "Customer");

        Summary(s =>
        {
            s.Summary = "Get paginated products";
            s.Description = "Returns paginated products filtered by category and optional filter criteria.";
            s.ExampleRequest = new GetProductsQuery(
                Guid.Parse("11111111-1111-1111-1111-111111111111"), 1, 10, new ProductFilter( "shirt", 10, 200, "M", "Black", "price", false));
            s.Responses[200] = "Products retrieved successfully.";
            s.Responses[400] = "Product query parameters are invalid.";
        });

        Description(x => x
            .Produces<PaginatedList<ProductDto>>(200)
            .ProducesProblemDetails(400));
    }

    public override async Task<IResult> ExecuteAsync(GetProductsRequest req, CancellationToken ct)
    {
        var query = new GetProductsQuery(req.CategoryId, req.PageNumber, req.PageSize,
            new ProductFilter(req.Search, req.MinPrice, req.MaxPrice, req.Size, req.Color, req.SortBy, req.Descending));

        var result = await _mediator.Send(query, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}

public sealed class GetProductsRequest
{
    public Guid CategoryId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; } = false;
}