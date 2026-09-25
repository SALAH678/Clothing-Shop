using Application.Common.Attributes;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.ResetPassword;

public sealed record ResetPasswordCommand(
    string Email,
    [property: Sensitive] string Code,
    [property: Sensitive] string NewPassword
) : IRequest<Result<string>>;

