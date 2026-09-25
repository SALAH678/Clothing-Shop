using Application.Common.Attributes;
using Application.Features.Authentications.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.Refresh;

public sealed record RefreshCommand([property: Sensitive] string refreshToken) : IRequest<Result<AuthResponse>>;
