using Api.Extensions;
using Application.Features.Authentications.Command.ResetPassword;
using FastEndpoints;
using MediatR;
using System.Threading.RateLimiting;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;
public class ResetPassword(IMediator mediator, [FromKeyedServices("auth-email-strict")] PartitionedRateLimiter<string> emailLimiter)
    : Endpoint<ResetPasswordCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/reset-password");
        Group<AuthGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Reset the user password";
            s.Description = "Verifies the password reset code and updates the account password to the new password supplied by the user.";
            s.ExampleRequest = new ResetPasswordCommand("john.doe@gmail.com", "123456", "N3wP@ssw0rd!");
            s.Responses[200] = "Password reset successfully.";
            s.Responses[400] = "The password reset payload or code is invalid.";
            s.Responses[401] = "The password reset request is invalid or expired.";
            s.Responses[404] = "A password reset token was not found.";
            s.Responses[500] = "Password reset could not be completed.";
        });

        Description(x => x
            .Produces<string>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(ResetPasswordCommand req, CancellationToken ct)
    {
        using var lease = await emailLimiter.AcquireAsync(req.Email, permitCount: 1, ct);
        if (!lease.IsAcquired)
        {
            var retryAfter = lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterMetadata)
                ? retryAfterMetadata
                : TimeSpan.FromSeconds(60);
            HttpContext.Response.Headers.Append("Retry-After", ((int)retryAfter.TotalSeconds).ToString());
            return Results.StatusCode(StatusCodes.Status429TooManyRequests);
        }

        var result = await _mediator.Send(req, ct);
        
        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
