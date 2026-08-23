using Api.Extensions;
using Application.Features.Authentications.Command.VerifyEmail;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;
public class VerifyEmail(IMediator mediator) : Endpoint<VerifyEmailCommand, Result<Success>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/verify-email");
        Group<AuthGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Verify the user email";
            s.Description = "Matches the email verification code sent to the user and completes the account verification flow.";
            s.ExampleRequest = new VerifyEmailCommand("john.doe@example.com", "123456");
            s.Responses[200] = "Email verified successfully.";
            s.Responses[400] = "The verification code is invalid.";
            s.Responses[401] = "The user is not valid for this verification request.";
            s.Responses[404] = "The verification token was not found.";
            s.Responses[409] = "The email address has already been verified.";
            s.Responses[500] = "Email verification could not be completed.";
        });

        Description(x => x
            .Produces<Success>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(409)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> HandleAsync(VerifyEmailCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
