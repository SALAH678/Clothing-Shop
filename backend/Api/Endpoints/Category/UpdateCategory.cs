using Api.Extensions;
using Application.Features.Categories.Commands.UpdateCategory;
using Application.Features.Categories.Dtos;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Category;

public class UpdateCategory(IMediator mediator) : Endpoint<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/{categoryId:guid}");
        Group<CategoryGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Update a category";
            s.Description = "Updates category details, including name and image.";
            s.ExampleRequest = new UpdateCategoryCommand(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "Men & Accessories",
                null,
                null);
            s.Responses[200] = "Category updated successfully.";
            s.Responses[400] = "Category update input is invalid.";
            s.Responses[404] = "Category was not found.";
            s.Responses[500] = "Category update failed.";
        });

        Description(x => x
            .Produces<CategoryDto>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> HandleAsync(UpdateCategoryCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
