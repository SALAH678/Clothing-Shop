namespace Application.Features.Carts.Dtos;

public record CartDto(
    Guid Id,
    decimal TotalAmount,
    IReadOnlyCollection<CartItemDto> Items);