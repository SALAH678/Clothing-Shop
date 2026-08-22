using Api.Extensions;
using Application.Features.Authentications.Command.VerifyEmail;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;
public class VerifyEmail(IMediator mediator) : Endpoint<VerifyEmailCommand, Result<Success>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("api/auth/verify-email");
        AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(VerifyEmailCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
