using Api.Extensions;
using Application.Features.Users.Dtos;
using Application.Features.Users.Query.GetAllUsers;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.User;

public class GetAllUsers(IMediator mediator) : EndpointWithoutRequest<IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("");
        Group<UserGroup>();
        Roles("Admin");

        Summary(s =>
        {
            s.Summary = "Get all users";
            s.Description = "Returns all users.";
            s.Responses[200] = "Users retrieved successfully.";
            s.Responses[401] = "Authentication is required.";
        });

        Description(x => x
            .Produces<List<UserDto>>(200)
            .ProducesProblemDetails(401));
    }

    public override async Task<IResult> ExecuteAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(), ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
