using Application.Features.Carts.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Carts.Queries.GetCart;

public sealed record GetCartQuery : IRequest<Result<CartDto>>;