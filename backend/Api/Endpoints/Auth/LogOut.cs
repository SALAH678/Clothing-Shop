using Api.Extensions;
using Application.Features.Authentications.Command.LogOut;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;
public class LogOut(IMediator mediator) : Endpoint<LogOutCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/logout");
        Group<AuthGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Sign out the current user";
            s.Description = "Invalidates the supplied refresh token and logs the user out of the current session.";
            s.ExampleRequest = new LogOutCommand("john.doe@gmail.com", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");
            s.Responses[200] = "User logged out successfully.";
            s.Responses[400] = "The logout payload is invalid.";
            s.Responses[401] = "The logout request is invalid, revoked, or expired.";
            s.Responses[404] = "The refresh token was not found.";
            s.Responses[500] = "Logout could not be completed.";
        });

        Description(x => x
            .Produces<Success>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }
    public override async Task<IResult> ExecuteAsync(LogOutCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
