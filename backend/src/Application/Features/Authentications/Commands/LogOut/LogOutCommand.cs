using Application.Common.Attributes;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.LogOut;

public sealed record LogOutCommand([property: Sensitive] string refreshToken) : IRequest<Result<Success>>;
