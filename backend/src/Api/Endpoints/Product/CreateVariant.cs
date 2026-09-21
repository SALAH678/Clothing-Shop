using Api.Extensions;
using Application.Features.Variants.Commands.CreateVariant;
using Application.Features.Variants.Dtos;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Product;

public class CreateVariant(IMediator mediator) : Endpoint<CreateVariantCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("{productId:guid}/variants");
        Group<ProductGroup>();
        Roles("Admin");
        Options(x => x.RequireRateLimiting("authenticated-write"));

        Summary(s =>
        {
            s.Summary = "Create a product variant";
            s.Description = "Creates a variant for an existing product.";
            s.ExampleRequest = new CreateVariantCommand(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "M",
                "Black",
                25);
            s.Responses[200] = "Variant created successfully.";
            s.Responses[400] = "Variant payload is invalid.";
            s.Responses[404] = "Product was not found.";
            s.Responses[409] = "Variant already exists for this product.";
            s.Responses[500] = "Variant creation failed.";
        });

        Description(x => x
            .Produces<VariantDto>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(409)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(CreateVariantCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem());
    }
}
