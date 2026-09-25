using Application.Common.Attributes;
using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Commands.RegisterExternalAuth;

public record RegisterExternalAuthCommand(
    [property: Sensitive] string IdToken,
    [property: Sensitive] string PhoneNumber
) : IRequest<Result<AuthResponse>>;