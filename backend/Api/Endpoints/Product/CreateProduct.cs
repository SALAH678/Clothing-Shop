using Api.Extensions;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Dtos;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Product;

public class CreateProduct(IMediator mediator) : Endpoint<CreateProductCommand, Result<ProductDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/");
        Group<ProductGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Create a product";
            s.Description = "Creates a product with optional variants and images.";
            s.ExampleRequest = new CreateProductCommand(
                "Basic T-Shirt",
                "Cotton t-shirt for daily use",
                49.99m,
                5m,
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                [new ProductVariantDto(Guid.Empty, "M", "Black", 25)],
                null);
            s.Responses[200] = "Product created successfully.";
            s.Responses[400] = "Product payload is invalid.";
            s.Responses[500] = "Product creation failed.";
        });

        Description(x => x
            .Produces<ProductDto>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> HandleAsync(CreateProductCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
