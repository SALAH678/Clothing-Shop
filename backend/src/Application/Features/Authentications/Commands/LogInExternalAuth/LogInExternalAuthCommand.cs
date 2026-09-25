using Application.Common.Attributes;
using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Commands.LogInExternalAuth;

public record LogInExternalAuthCommand(
    [property: Sensitive] string IdToken
) : IRequest<Result<AuthResponse>>;