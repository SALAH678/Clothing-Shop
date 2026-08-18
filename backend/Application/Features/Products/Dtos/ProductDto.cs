namespace Application.Features.Products.Dtos;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal BasePrice,
    decimal? Discount,
    Guid CategoryId,
    List<ProductVariantDto> Variants);
