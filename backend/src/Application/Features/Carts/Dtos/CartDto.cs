namespace Application.Features.Carts.Dtos;

public class CartDto
{
    public Guid Id { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyCollection<CartItemDto> Items { get; init; } = [];
}