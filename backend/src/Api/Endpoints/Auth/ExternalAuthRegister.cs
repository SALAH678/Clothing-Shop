using Api.Extensions;
using Application.Features.Authentications.Commands.RegisterExternalAuth;
using FastEndpoints;
using MediatR;

namespace Api.Endpoints.Auth;

public class ExternalAuthRegister(IMediator mediator) : Endpoint<RegisterExternalAuthCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/google/register");
        Group<AuthGroup>();
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("auth-ip-relaxed"));

        Summary(s =>
        {
            s.Summary = "Register via Google OAuth";

            s.Description =
                "Validates a Google ID token and creates a new user account " +
                "using the verified Google email and provided phone number. " +
                "The Google account is linked to the newly created user. " +
                "Issues an access token and refresh token.";

            s.ExampleRequest = new RegisterExternalAuthCommand("eyJhbGciOiJSUzI1NiIsImtpZCI6...", "+213555123456");

            s.Responses[200] = "Google registration succeeded; access token returned and refresh token set as an HttpOnly cookie.";
            s.Responses[400] = "The Google ID token is invalid, the Google email is not verified, the phone number is invalid, or a user or Google account already exists.";
            s.Responses[500] = "The Google registration request could not be processed.";
        });

        Description(x => x
            .Produces<LoginResponse>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(RegisterExternalAuthCommand req, CancellationToken ct)
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
