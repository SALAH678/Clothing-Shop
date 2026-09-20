using Api.Extensions;
using Application.Features.Users.Command.UpdateCurrentUserProfile;
using Application.Features.Users.Dtos;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.User;

public class UpdateCurrentUserProfile(IMediator mediator) : Endpoint<UpdateCurrentUserProfileCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Put("/current");
        Group<UserGroup>();
        Roles("Admin", "Customer");
        Options(x => x.RequireRateLimiting("admin-write"));

        Summary(s =>
        {
            s.Summary = "Update current user profile";
            s.Description = "Updates the current user's profile fields.";
            s.ExampleRequest = new UpdateCurrentUserProfileCommand(
                "John",
                "Doe",
                "0123456789");
            s.Responses[200] = "User profile updated successfully.";
            s.Responses[401] = "Authentication is required.";
            s.Responses[400] = "User profile update payload is invalid.";
            s.Responses[404] = "Current user was not found.";
            s.Responses[500] = "User profile update failed.";
        });

        Description(x => x
            .Produces<UserDto>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(UpdateCurrentUserProfileCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
