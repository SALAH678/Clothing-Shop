using Api.Extensions;
using Application.Features.Categories.Dtos;
using Application.Features.Categories.Queries.GetCategoryById;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Category;

public class GetCategoryById(IMediator mediator) : Endpoint<GetCategoryByIdQuery, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/{categoryId:guid}");
        Group<CategoryGroup>();
        Roles("Admin", "Customer");

        Summary(s =>
        {
            s.Summary = "Get category by id";
            s.Description = "Returns a specific category by id.";
            s.ExampleRequest = new GetCategoryByIdQuery(Guid.Parse("11111111-1111-1111-1111-111111111111"));
            s.Responses[200] = "Category retrieved successfully.";
            s.Responses[404] = "Category was not found.";
        });

        Description(x => x
            .Produces<CategoryDto>(200)
            .ProducesProblemDetails(404));
    }

    public override async Task<IResult> ExecuteAsync(GetCategoryByIdQuery req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
