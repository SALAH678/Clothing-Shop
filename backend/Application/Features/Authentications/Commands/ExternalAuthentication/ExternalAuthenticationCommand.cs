using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.ExternalAuthentication;

public record ExternalAuthenticationCommand(
    string Provider,
    string IdToken
) : IRequest<Result<AuthResponse>>;


