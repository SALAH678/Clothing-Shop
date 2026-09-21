using Api.Extensions;
using Application.Features.Authentications.Command.VerifyEmail;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using System.Threading.RateLimiting;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;
public class VerifyEmail(IMediator mediator, [FromKeyedServices("auth-email-strict")] PartitionedRateLimiter<string> emailLimiter)
    : Endpoint<VerifyEmailCommand, IResult>
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
            s.ExampleRequest = new VerifyEmailCommand("john.doe@gmail.com", "123456");
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

    public override async Task<IResult> ExecuteAsync(VerifyEmailCommand req, CancellationToken ct)
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
