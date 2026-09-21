using Api.Extensions;
using Application.Features.Authentications.Command.ForgotPassword;
using FastEndpoints;
using MediatR;
using System.Threading.RateLimiting;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;
public class ForgotPassword(IMediator mediator, [FromKeyedServices("auth-target-strict")] PartitionedRateLimiter<string> targetLimiter)
    : Endpoint<ForgotPasswordCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/forgot-password");
        Group<AuthGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Request a password reset";
            s.Description = "Sends a password reset email to the user so they can create a new password.";
            s.ExampleRequest = new ForgotPasswordCommand("john.doe@gmail.com");
            s.Responses[200] = "Password reset instructions sent successfully.";
            s.Responses[400] = "The email address is invalid.";
            s.Responses[500] = "Password reset request could not be processed.";
        });

        Description(x => x
            .Produces<string>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(ForgotPasswordCommand req, CancellationToken ct)
    {
        using var lease = await targetLimiter.AcquireAsync(req.Email, permitCount: 1, ct);
        if (!lease.IsAcquired)
            return Results.StatusCode(StatusCodes.Status429TooManyRequests);

        var result = await _mediator.Send(req, ct);
        
        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
