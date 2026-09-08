using Application.Features.Carts.Dtos;
using Application.Features.Products.Dtos;
using Application.Features.Variants.Dtos;
using AutoMapper;
using Domain.Carts;
using Domain.Carts.CartItems;
using Domain.Products;
using Domain.Products.Variants;

namespace Application.Features.Carts.Mappers;

public class CartProfile : Profile
{
    public CartProfile()
    {
        CreateMap<Product, CartProductDto>()
            .ForMember(
                dest => dest.ImageUrl,
                opt => opt.MapFrom(src => ResolveImageUrl(src)))
            .ForMember(
                dest => dest.BasePrice,
                opt => opt.MapFrom(src => src.BasePrice))
            .ForMember(
                dest => dest.Discount,
                opt => opt.MapFrom(src => src.Discount));

        CreateMap<Variant, CartVariantDto>();

        CreateMap<CartItem, CartItemDto>();

        CreateMap<Cart, CartDto>()
            .ForMember(
                dest => dest.TotalAmount,
                opt => opt.MapFrom(src => src.Items != null
                    ? src.Items.Sum(item => item.Quantity * item.Variant.Product.BasePrice)
                    : 0));
    }

    private static string? ResolveImageUrl(Product product)
    {
        if (product.Images is null)
            return null;

        return product.Images.FirstOrDefault(image => image.IsMain)?.ImageUrl
            ?? product.Images.FirstOrDefault()?.ImageUrl;
    }
}