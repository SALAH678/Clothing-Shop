using Application.Common.Models;
using AutoMapper;

namespace Application.Features.Products.Mappers;

public class PaginationProfile : Profile
{
    public PaginationProfile()
    {
        CreateMap(typeof(PaginatedList<>), typeof(PaginatedList<>));
    }
}
