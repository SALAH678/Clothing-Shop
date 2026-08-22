using Api.Extensions;
using Application.Features.Authentications.Command.LogOut;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;
public class LogOut(IMediator mediator) : Endpoint<LogOutCommand, Result<Success>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("api/auth/logout");
        AllowAnonymous();
    }
    public override async Task<IResult> HandleAsync(LogOutCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
