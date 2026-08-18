using Application.Features.Products.Dtos;
using AutoMapper;
using Domain.Products;
using Domain.Products.Variants;

namespace Application.Features.Products.Mappers;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Variant, ProductVariantDto>();

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Variants,
                opt => opt.MapFrom(src => src.Variants));
    }
}
