using Application.Features.Carts.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Carts.Commands.AddCartItem;

public sealed record AddCartItemCommand(
    Guid VariantId,
    int Quantity) : IRequest<Result<CartDto>>;