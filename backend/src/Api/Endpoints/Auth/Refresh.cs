using Api.Extensions;
using Application.Features.Authentications.Command.Refresh;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;

public class Refresh(IMediator mediator) : EndpointWithoutRequest<IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/refresh");
        Group<AuthGroup>();
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("auth-ip-relaxed"));

        Summary(s =>
        {
            s.Summary = "Refresh the access token";
            s.Description = "Issues a new access token using the user email and refresh token when the current token has expired or is close to expiring.";
            
            s.Responses[200] = "New access and refresh tokens generated successfully.";
            s.Responses[400] = "The refresh payload is invalid.";
            s.Responses[401] = "The refresh token is invalid, revoked, or expired.";
            s.Responses[404] = "The refresh token was not found.";
            s.Responses[500] = "Token refresh could not be completed.";
        });

        Description(x => x
            .Produces<LoginResponse>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(CancellationToken ct)
    {
        if (!HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshTokenFromCookie) || string.IsNullOrWhiteSpace(refreshTokenFromCookie))
            return Results.Problem(statusCode: 401, detail: "No refresh token cookie present.");

        var command = new RefreshCommand(refreshTokenFromCookie);
        var result = await _mediator.Send(command, ct);

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
