using Application.Common.Attributes;
using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.Login;

public sealed record LoginCommand(
        string Email,
        [property: Sensitive] string Password
    ) : IRequest<Result<AuthResponse>>;
