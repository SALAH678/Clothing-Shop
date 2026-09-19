using Application.Features.Variants.Dtos;
using AutoMapper;
using Domain.Products.Variants;

namespace Application.Features.Variants.Mappers;

public sealed class VariantProfile : Profile
{
    public VariantProfile()
    {
        CreateMap<Variant, VariantDto>();
    }
}