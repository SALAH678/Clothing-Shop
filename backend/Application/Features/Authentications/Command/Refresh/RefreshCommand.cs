using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.Refresh;

public sealed record RefreshCommand(
        string email,
        string refreshToken) : IRequest<Result<AuthResponse>>;
