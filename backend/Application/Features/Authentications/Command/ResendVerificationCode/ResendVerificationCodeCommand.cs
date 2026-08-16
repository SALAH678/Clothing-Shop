using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.ResendVerificationCode;

public record ResendVerificationCodeCommand(
    string Email
) : IRequest<Result<string>>;
