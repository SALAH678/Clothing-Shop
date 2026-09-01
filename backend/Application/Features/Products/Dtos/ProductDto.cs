using Application.Features.Variants.Dtos;

namespace Application.Features.Products.Dtos;

public class ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal BasePrice { get; init; }
    public decimal? Discount { get; init; }
    public Guid CategoryId { get; init; }
    public List<VariantDto> Variants { get; init; } = [];
    public List<string> ImageUrls { get; init; } = [];
}
