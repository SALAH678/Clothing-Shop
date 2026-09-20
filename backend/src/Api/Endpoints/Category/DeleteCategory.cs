using Api.Extensions;
using Application.Features.Categories.Commands.DeleteCategory;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Category;

public class DeleteCategory(IMediator mediator) : Endpoint<DeleteCategoryCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Delete("/{categoryId:guid}");
        Group<CategoryGroup>();
        Roles("Admin");
        Options(x => x.RequireRateLimiting("admin-write"));

        Summary(s =>
        {
            s.Summary = "Delete a category";
            s.Description = "Deletes a category by id and schedules image cleanup if needed.";
            s.ExampleRequest = new DeleteCategoryCommand(Guid.Parse("11111111-1111-1111-1111-111111111111"));
            s.Responses[200] = "Category deleted successfully.";
            s.Responses[404] = "Category was not found.";
            s.Responses[500] = "Category deletion failed.";
        });

        Description(x => x
            .Produces<Deleted>(200)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(DeleteCategoryCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
