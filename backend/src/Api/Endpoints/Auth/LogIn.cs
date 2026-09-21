using Api.Extensions;
using Application.Features.Authentications.Command.Login;
using Application.Features.Users.Dtos;
using FastEndpoints;
using MediatR;
using System.Threading.RateLimiting;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;

public record LoginResponse(UserDto User, string AccessToken);

public class LogIn(IMediator mediator, [FromKeyedServices("auth-email-strict")] PartitionedRateLimiter<string> emailLimiter)
    : Endpoint<LoginCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/login");
        Group<AuthGroup>();
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("auth-ip-spray-guard"));

        Summary(s =>
        {
            s.Summary = "Sign in to the application";
            s.Description = "Authenticates a user with email and password and returns the user profile plus a fresh access and refresh token pair.";
            s.ExampleRequest = new LoginCommand("john.doe@gmail.com", "P@ssw0rd123");
            s.Responses[200] = "User authenticated successfully.";
            s.Responses[400] = "The login payload is invalid.";
            s.Responses[401] = "The supplied credentials are invalid or the email is not verified.";
            s.Responses[500] = "Authentication failed while creating the session.";
        });

        Description(x => x
            .Produces<LoginResponse>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(500));
    }
    public override async Task<IResult> ExecuteAsync(LoginCommand req, CancellationToken ct)
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
            onSuccess: value =>
            {
                HttpContext.Response.Cookies.Append("refreshToken", value.Tokens.RefreshToken!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/api/auth",
                    Expires = value.Tokens.RefreshTokenExpiresAtUtc
                });
                var response = new LoginResponse(value.User, value.Tokens.AccessToken!);
                return Results.Ok(response);
            },
            onError: errors => errors.ToProblem()
        );
    }
}
