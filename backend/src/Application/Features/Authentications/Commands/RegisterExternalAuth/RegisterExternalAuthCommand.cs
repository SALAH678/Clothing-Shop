using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Commands.RegisterExternalAuth;

public record RegisterExternalAuthCommand(
    string IdToken,
    string PhoneNumber
) : IRequest<Result<AuthResponse>>;