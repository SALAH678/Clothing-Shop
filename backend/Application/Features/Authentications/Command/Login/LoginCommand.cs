using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.Login;

public sealed record LoginCommand(
        string Email,
        string Password
    ) : IRequest<Result<AuthResponse>>;
