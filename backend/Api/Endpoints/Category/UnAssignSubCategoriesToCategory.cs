using Api.Extensions;
using Application.Features.Categories.Commands.UnAssignSubCategoriesToCategory;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Category;

public class UnAssignSubCategoriesToCategory(IMediator mediator) : Endpoint<UnAssignSubCategoriesToCategoryCommand, Result<Success>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Delete("/{categoryId:guid}/subcategories");
        Group<CategoryGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Unassign subcategories from a category";
            s.Description = "Removes parent-category assignment from one or more subcategories.";
            s.ExampleRequest = new UnAssignSubCategoriesToCategoryCommand(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                [
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Guid.Parse("33333333-3333-3333-3333-333333333333")
                ]);
            s.Responses[200] = "Subcategories unassigned successfully.";
            s.Responses[400] = "Unassignment request is invalid.";
            s.Responses[404] = "Parent category or one or more subcategories were not found.";
            s.Responses[409] = "Unassignment request contains a circular-reference constraint violation.";
            s.Responses[500] = "Subcategory unassignment failed.";
        });

        Description(x => x
            .Produces<Success>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(409)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> HandleAsync(UnAssignSubCategoriesToCategoryCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
