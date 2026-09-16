using Api.Extensions;
using Application.Features.Categories.Dtos;
using Application.Features.Categories.Queries.GetCategories;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Category;

public class GetCategories(IMediator mediator) : EndpointWithoutRequest<IResult>
{
    
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("");
        Group<CategoryGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get all categories";
            s.Description = "Returns all categories with their hierarchy information.";
            s.Responses[200] = "Categories retrieved successfully.";
        });

        Description(x => x
            .Produces<List<CategoryDto>>(200));
    }

    public override async Task<IResult> ExecuteAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
