using Api.Extensions;
using Application.Common.Models;
using Application.Features.Users.Dtos;
using Application.Features.Users.Query.GetAllUsers;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.User;

public class GetAllUsers(IMediator mediator) : Endpoint<GetAllUsersQuery, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("");
        Group<UserGroup>();
        Roles("Admin");
        Options(x => x.RequireRateLimiting("admin-read"));

        Summary(s =>
        {
            s.Summary = "Get all users";
            s.Description =
                "Returns a paginated list of all registered users. \n" +
                "Results are paginated using PageNumber and PageSize. \n" +
                "This endpoint is restricted to administrators.";

            s.Responses[200] = "Users retrieved successfully.";
            s.Responses[400] = "The pagination parameters are invalid.";
            s.Responses[401] = "Authentication is required.";
            s.Responses[403] = "The authenticated user does not have permission to access this resource.";
        });

        Description(x => x
            .Produces<PaginatedList<UserDto>>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(403));
    }

    public override async Task<IResult> ExecuteAsync(GetAllUsersQuery request, CancellationToken ct)
    {
        var result = await _mediator.Send(request, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
