using Application.Features.Carts.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Carts.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid CartItemId) : IRequest<Result<Deleted>>;