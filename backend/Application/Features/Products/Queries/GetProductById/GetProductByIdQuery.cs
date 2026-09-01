using Application.Features.Products.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ProductDto>>;
