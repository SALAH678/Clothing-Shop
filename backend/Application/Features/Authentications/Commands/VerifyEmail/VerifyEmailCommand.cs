using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.VerifyEmail;

public sealed record VerifyEmailCommand(
        string Email,
        string Code)
        : IRequest<Result<AuthResponse>>;
