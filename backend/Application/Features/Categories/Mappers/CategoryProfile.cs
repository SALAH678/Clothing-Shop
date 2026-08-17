using Application.Features.Categories.Dtos;
using AutoMapper;
using Domain.Categories;

namespace Application.Features.Categories.Mappers
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryDto>()
                .ForMember(
                dest => dest.Subcategories,
                opt => opt.MapFrom(src => src.Subcategories))// this means that the Subcategories property of the
                                                             // CategoryDto will be populated with the Subcategories property of the Category entity.(u're telling him what will do manually)
                .MaxDepth(3);
        }
    }
}
