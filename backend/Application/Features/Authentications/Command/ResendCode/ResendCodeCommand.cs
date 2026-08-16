using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.ResendVerificationCode;

public record ResendCodeCommand(
    string Email,
    string VerificationTokenType
) : IRequest<Result<string>>;
