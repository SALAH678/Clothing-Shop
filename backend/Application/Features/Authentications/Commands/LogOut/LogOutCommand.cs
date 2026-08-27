using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.LogOut;

public sealed record LogOutCommand(
        string email,
        string refreshToken) : IRequest<Result<Success>>;
