using Api.Extensions;
using Application.Features.Users.Dtos;
using Application.Features.Users.Query.GetCurrentUserProfile;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.User;

public class GetCurrentUserProfile(IMediator mediator) : EndpointWithoutRequest<Result<UserDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/current");
        Group<UserGroup>();

        Summary(s =>
        {
            s.Summary = "Get current user profile";
            s.Description = "Returns the profile of the authenticated user.";
            s.Responses[200] = "Current user profile retrieved successfully.";
            s.Responses[401] = "Authentication is required.";
            s.Responses[404] = "Current user was not found.";
        });

        Description(x => x
            .Produces<UserDto>(200)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404));
    }

    public override async Task<IResult> HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentUserProfileQuery(), ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
