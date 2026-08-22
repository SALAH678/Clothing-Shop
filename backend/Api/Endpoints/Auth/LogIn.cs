using Api.Extensions;
using Application.Features.Authentications.Command.Login;
using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;

public class LogIn(IMediator mediator) : Endpoint<LoginCommand, Result<AuthResponse>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("api/auth/login");
        AllowAnonymous();
    }
    public override async Task<IResult> HandleAsync(LoginCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
