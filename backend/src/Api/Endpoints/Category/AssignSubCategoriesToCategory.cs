using Api.Extensions;
using Application.Features.Categories.Commands.AssignSubCategoriesToCategory;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Category;

public class AssignSubCategoriesToCategory(IMediator mediator) : Endpoint<AssignSubCategoriesToCategoryCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/{categoryId:guid}/subcategories");
        Group<CategoryGroup>();
        Roles("Admin");
        Options(x => x.RequireRateLimiting("authenticated-write"));

        Summary(s =>
        {
            s.Summary = "Assign subcategories to a category";
            s.Description = "Assigns one or more subcategories to a parent category.";
            s.ExampleRequest = new AssignSubCategoriesToCategoryCommand(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                [
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Guid.Parse("33333333-3333-3333-3333-333333333333")
                ]);
            s.Responses[200] = "Subcategories assigned successfully.";
            s.Responses[400] = "Assignment request is invalid.";
            s.Responses[404] = "Parent category or one or more subcategories were not found.";
            s.Responses[409] = "Assignment would create a circular category reference.";
            s.Responses[500] = "Subcategory assignment failed.";
        });

        Description(x => x
            .Produces<Success>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(409)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(AssignSubCategoriesToCategoryCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
