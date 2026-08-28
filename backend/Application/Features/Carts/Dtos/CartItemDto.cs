using Application.Features.Variants.Dtos;

namespace Application.Features.Carts.Dtos;

public class CartItemDto
{
    public Guid Id { get; init; }
    public int Quantity { get; init; }
    public CartVariantDto Variant { get; init; } = null!;
}