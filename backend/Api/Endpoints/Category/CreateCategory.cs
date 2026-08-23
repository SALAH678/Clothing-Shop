using Api.Extensions;
using Application.Features.Categories.Commands.CreateCategory;
using Application.Features.Categories.Dtos;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Category;

public class CreateCategory(IMediator mediator) : Endpoint<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/");
        Group<CategoryGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Create a category";
            s.Description = "Creates a new category with an optional image.";
            s.ExampleRequest = new CreateCategoryCommand(
                "T-Shirts",
                null,
                null);
            s.Responses[200] = "Category created successfully.";
            s.Responses[400] = "Category input is invalid.";
            s.Responses[500] = "Category creation failed.";
        });

        Description(x => x
            .Produces<CategoryDto>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> HandleAsync(CreateCategoryCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}


    

