using Api.Extensions;
using Application.Features.Authentications.Command.Refresh;
using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;

public class Refresh(IMediator mediator) : Endpoint<RefreshCommand, Result<AuthResponse>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/refresh");
        Group<AuthGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Refresh the access token";
            s.Description = "Issues a new access token using the user email and refresh token when the current token has expired or is close to expiring.";
            s.ExampleRequest = new RefreshCommand("john.doe@example.com", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");
            s.Responses[200] = "New access and refresh tokens generated successfully.";
            s.Responses[400] = "The refresh payload is invalid.";
            s.Responses[401] = "The refresh token is invalid, revoked, or expired.";
            s.Responses[404] = "The refresh token was not found.";
            s.Responses[500] = "Token refresh could not be completed.";
        });

        Description(x => x
            .Produces<AuthResponse>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> HandleAsync(RefreshCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
