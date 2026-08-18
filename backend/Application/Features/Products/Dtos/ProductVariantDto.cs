namespace Application.Features.Products.Dtos;

public record ProductVariantDto(
    Guid Id,
    string Size,
    string Color,
    int StockQuantity);
