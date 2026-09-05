using Api.Extensions;
using Application.Features.Authentications.Command.Login;
using Application.Features.Authentications.Dtos;
using Application.Features.Users.Dtos;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;

public record LoginResponse(UserDto User, string AccessToken);

public class LogIn(IMediator mediator) : Endpoint<LoginCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/login");
        Group<AuthGroup>();
        AllowAnonymous();

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
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value =>
            {
                HttpContext.Response.Cookies.Append("refreshToken", value.Tokens.RefreshToken!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/api/auth/refresh",
                    //Expires = new DateTimeOffset(value.Tokens.RefreshTokenExpiresAtUtc.UtcDateTime, TimeSpan.Zero)
                    Expires = value.Tokens.RefreshTokenExpiresAtUtc
                });
                var response = new LoginResponse(value.User, value.Tokens.AccessToken!);
                return Results.Ok(response);
            },
            onError: errors => errors.ToProblem()
        );
    }
}
