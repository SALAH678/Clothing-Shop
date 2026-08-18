using Application.Features.Carts.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Carts.Commands.CreateCart;

public sealed record CreateCartCommand : IRequest<Result<CartDto>>;