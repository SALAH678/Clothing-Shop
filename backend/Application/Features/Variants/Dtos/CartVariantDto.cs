using Application.Features.Products.Dtos;

namespace Application.Features.Variants.Dtos;

public record CartVariantDto(
    Guid Id,
    string Size,
    string Color,
    int StockQuantity,
    CartProductDto Product);