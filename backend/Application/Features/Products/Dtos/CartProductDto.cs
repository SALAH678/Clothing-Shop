namespace Application.Features.Products.Dtos;

public class CartProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal BasePrice { get; init; }
    public decimal Discount { get; init; }
}