using Application.Features.Products.Dtos;

namespace Application.Features.Variants.Dtos;

public class CartVariantDto
{
    public Guid Id { get; init; }
    public string Size { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public CartProductDto Product { get; init; } = null!;
}