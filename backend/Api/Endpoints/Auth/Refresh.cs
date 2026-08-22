using Api.Extensions;
using Application.Features.Authentications.Command.Refresh;
using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;

public class Refresh(IMediator mediator) : Endpoint<RefreshCommand, Result<AuthResponse>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("api/auth/refresh");
        AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(RefreshCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
