using Api.Extensions;
using Application.Features.Authentications.Command.ForgotPassword;
using Domain.Common.Results;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Auth;
public class ForgotPassword(IMediator mediator) : Endpoint<ForgotPasswordCommand, Result<string>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("api/auth/forgot-password");
        AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(ForgotPasswordCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        
        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }
}
