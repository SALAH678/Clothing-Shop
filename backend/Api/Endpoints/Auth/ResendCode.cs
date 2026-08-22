using Api.Extensions;
using Application.Features.Authentications.Command.ResendVerificationCode;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;
public class ResendCode(IMediator mediator) : Endpoint<ResendCodeCommand, Result<string>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("api/auth/resend-code");
        AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(ResendCodeCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
