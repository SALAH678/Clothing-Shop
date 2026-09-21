using Api.Extensions;
using Application.Features.Authentications.Command.ResendVerificationCode;
using FastEndpoints;
using MediatR;
using System.Threading.RateLimiting;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;
public class ResendCode(IMediator mediator, [FromKeyedServices("auth-target-strict")] PartitionedRateLimiter<string> targetLimiter)
    : Endpoint<ResendCodeCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/resend-code");
        Group<AuthGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Resend the verification code";
            s.Description = "Sends a fresh verification email or message for the selected verification token type.";
            s.ExampleRequest = new ResendCodeCommand("john.doe@gmail.com", "EmailVerification || PhoneVerification");
            s.Responses[200] = "Verification code sent successfully.";
            s.Responses[400] = "The verification request is invalid.";
            s.Responses[401] = "The verification request is unauthorized.";
            s.Responses[409] = "The email address has already been verified.";
            s.Responses[500] = "Verification code could not be resent.";
        });

        Description(x => x
            .Produces<string>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(409)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(ResendCodeCommand req, CancellationToken ct)
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
