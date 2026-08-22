using Api.Extensions;
using Application.Features.Authentications.Command.ResetPassword;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;
public class ResetPassword(IMediator mediator) : Endpoint<ResetPasswordCommand, Result<string>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("api/auth/reset-password");
        AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(ResetPasswordCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        
        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
