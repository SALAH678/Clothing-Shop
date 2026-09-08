using Api.Extensions;
using Application.Features.Authentications.Command.LogOut;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;
public class LogOut(IMediator mediator) : EndpointWithoutRequest<IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/logout");
        Group<AuthGroup>();
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Sign out the current user";
            s.Description = "Invalidates the refresh token cookie and logs the user out of the current session.";

            s.Responses[200] = "User logged out successfully.";
            s.Responses[400] = "The logout payload is invalid.";
            s.Responses[401] = "The logout request is invalid, revoked, or expired.";
            s.Responses[404] = "The refresh token was not found.";
            s.Responses[500] = "Logout could not be completed.";
        });

        Description(x => x
            .Produces<Success>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }
    public override async Task<IResult> ExecuteAsync(CancellationToken ct)
    {
        if (!HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            ClearRefreshTokenCookie();
            return Results.Ok();
        }

        var result = await _mediator.Send(new LogOutCommand(refreshToken), ct);

        if (result.IsSuccess)
        {
            ClearRefreshTokenCookie();
            return Results.Ok();
        }

        return result.Errors.ToProblem();
    }

    private void ClearRefreshTokenCookie()
    {
        HttpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/api/auth"
            }
        );
    }
}
