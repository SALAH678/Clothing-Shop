using Api.Extensions;
using Application.Features.Authentications.Command.Register;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;
public class Register(IMediator mediator) : Endpoint<RegisterCommand,Result<string>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/register");
        Group<AuthGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Register a new account";
            s.Description = "Creates a new user account and immediately sends a verification email to the provided address.";
            s.ExampleRequest = new RegisterCommand(
                "John",
                "Doe",
                "+1234567890",
                "john.doe@example.com",
                "P@ssw0rd123");
            s.Responses[200] = "Registration successful.";
            s.Responses[400] = "Validation failed for the registration payload.";
            s.Responses[409] = "The provided email address is already in use.";
            s.Responses[500] = "Registration could not be completed.";
        });

        Description(x => x
            .Produces<string>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(409)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> HandleAsync(RegisterCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
