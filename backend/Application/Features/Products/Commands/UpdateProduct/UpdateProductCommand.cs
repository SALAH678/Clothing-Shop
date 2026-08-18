using Application.Features.Products.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ProductId,
    string? Name,
    string? Description,
    decimal? BasePrice,
    decimal? Discount) : IRequest<Result<ProductDto>>;
