using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.ForgotPassword;

public record ForgotPasswordCommand(
    string Email) : IRequest<Result<string>>;


