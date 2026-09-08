using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.ExternalAuthentication;

public record ExternalAuthCommand(string IdToken, string phoneNumber) : IRequest<Result<AuthResponse>>;


