namespace Application.Features.Products.Dtos;

public class ProductVariantDto
{
    public string Size { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
}
