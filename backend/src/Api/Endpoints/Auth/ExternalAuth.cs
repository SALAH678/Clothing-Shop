using Api.Extensions;
using Application.Features.Authentications.Command.ExternalAuthentication;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;

public class ExternalAuth(IMediator mediator) : Endpoint<ExternalAuthCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/google");
        Group<AuthGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Log in or register via Google OAuth";
            s.Description = "Validates a Google ID token and either logs in the existing user or registers a new one, then issues access and refresh tokens.";
            s.ExampleRequest = new ExternalAuthCommand("eyJhbGciOiJSUzI1NiIsImtpZCI6...", "+1234567890");
            s.Responses[200] = "Login or registration succeeded; access token returned and refresh token set as an HttpOnly cookie.";
            s.Responses[400] = "The Google ID token is invalid, the email is not verified, or the provided data failed validation.";
            s.Responses[500] = "The OAuth login request could not be processed.";
        });

        Description(x => x
            .Produces<LoginResponse>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(ExternalAuthCommand req, CancellationToken ct)
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
