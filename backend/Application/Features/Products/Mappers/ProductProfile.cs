using Application.Features.Products.Dtos;
using Application.Features.Variants.Dtos;
using AutoMapper;
using Domain.Products;
using Domain.Products.Images;
using Domain.Products.Variants;

namespace Application.Features.Products.Mappers;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Variant, VariantDto>();
        CreateMap<Image, ImageDto>();

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Variants,
                opt => opt.MapFrom(src => src.Variants))
            .ForMember(dest => dest.Images,
                opt => opt.MapFrom(src => src.Images));
    }
}
