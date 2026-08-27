using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.ResetPassword;

public sealed record ResetPasswordCommand(
    string Email,
    string Code,
    string NewPassword
) : IRequest<Result<string>>;

