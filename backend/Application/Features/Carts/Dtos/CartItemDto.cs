using Application.Features.Variants.Dtos;

namespace Application.Features.Carts.Dtos;

public record CartItemDto(
    Guid Id,
    int Quantity,
    CartVariantDto Variant);