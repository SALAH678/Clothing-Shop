using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Commands.LogInExternalAuth;

public record LogInExternalAuthCommand(
    string IdToken
) : IRequest<Result<AuthResponse>>;