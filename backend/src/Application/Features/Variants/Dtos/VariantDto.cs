
namespace Application.Features.Variants.Dtos;

public class VariantDto
{
    public Guid Id { get; init; }
    public string Size { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
}
