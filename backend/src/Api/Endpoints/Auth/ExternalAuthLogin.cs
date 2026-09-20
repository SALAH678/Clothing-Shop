using Api.Extensions;
using Application.Features.Authentications.Commands.LogInExternalAuth;
using FastEndpoints;
using MediatR;

namespace Api.Endpoints.Auth;

public class ExternalAuthLogin(IMediator mediator) : Endpoint<LogInExternalAuthCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/google/login");
        Group<AuthGroup>();
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("auth-ip-relaxed"));

        Summary(s =>
        {
            s.Summary = "Log in via Google OAuth";

            s.Description =
                "Validates a Google ID token and authenticates the user. " +
                "If the Google account is not already linked, a verified Google email " +
                "may be used to automatically link it to an existing user account. " +
                "Issues a new access token and refresh token.";

            s.ExampleRequest = new LogInExternalAuthCommand("eyJhbGciOiJSUzI1NiIsImtpZCI6...");

            s.Responses[200] = "Google login succeeded; access token returned and refresh token set as an HttpOnly cookie.";
            s.Responses[400] =  "The Google ID token is invalid, the Google email is not verified, or the login request failed validation.";
            s.Responses[500] = "The Google login request could not be processed.";
        });

        Description(x => x
            .Produces<LoginResponse>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(500));
    }
    
    public override async Task<IResult> ExecuteAsync(LogInExternalAuthCommand req, CancellationToken ct)
    {
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
