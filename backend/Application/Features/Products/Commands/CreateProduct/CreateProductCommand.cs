using Application.Features.Products.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal BasePrice,
    decimal? Discount,
    Guid CategoryId,
    List<ProductVariantDto>? Variants) : IRequest<Result<ProductDto>>;
