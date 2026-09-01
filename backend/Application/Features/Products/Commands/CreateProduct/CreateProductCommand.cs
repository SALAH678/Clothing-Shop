using Application.Features.Products.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed record ImageDto(
    string fileName,
    Stream ImageContent,
    bool IsMain
);
public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal BasePrice,
    decimal? Discount,
    Guid CategoryId,
    List<ProductVariantDto>? Variants,
    List<ImageDto>? Images) : IRequest<Result<ProductDto>>;
