using Api.Extensions;
using Application.Features.Variants.Commands.UpdateVariant;
using Application.Features.Variants.Dtos;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Product;

public class UpdateVariant(IMediator mediator) : Endpoint<UpdateVariantCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/variants/{variantId:guid}");
        Group<ProductGroup>();
        Roles("Admin");

        Summary(s =>
        {
            s.Summary = "Update a product variant";
            s.Description = "Updates the size, color, or stock quantity of a product variant.";
            s.ExampleRequest = new UpdateVariantCommand(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                "L",
                "Navy",
                30);
            s.Responses[200] = "Variant updated successfully.";
            s.Responses[400] = "Variant update payload is invalid.";
            s.Responses[404] = "Variant was not found.";
            s.Responses[500] = "Variant update failed.";
        });

        Description(x => x
            .Produces<VariantDto>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(UpdateVariantCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem());
    }
}
