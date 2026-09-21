using Api.Extensions;
using Application.Features.Variants.Commands.DeleteVariant;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Product;

public class DeleteVariant(IMediator mediator) : Endpoint<DeleteVariantCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Delete("/variants/{variantId:guid}");
        Group<ProductGroup>();
        Roles("Admin");
        Options(x => x.RequireRateLimiting("authenticated-write"));

        Summary(s =>
        {
            s.Summary = "Delete a product variant";
            s.Description = "Deletes a product variant by id.";
            s.ExampleRequest = new DeleteVariantCommand(
                Guid.Parse("22222222-2222-2222-2222-222222222222"));
            s.Responses[200] = "Variant deleted successfully.";
            s.Responses[400] = "Variant id is invalid.";
            s.Responses[404] = "Variant was not found.";
            s.Responses[500] = "Variant deletion failed.";
        });

        Description(x => x
            .Produces<Deleted>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(DeleteVariantCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem());
    }
}
