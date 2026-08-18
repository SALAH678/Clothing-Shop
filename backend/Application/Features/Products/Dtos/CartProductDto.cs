namespace Application.Features.Products.Dtos;

public record CartProductDto(
    Guid Id,
    string Name,
    string? ImageUrl,
    decimal BasePrice,
    decimal Discount);