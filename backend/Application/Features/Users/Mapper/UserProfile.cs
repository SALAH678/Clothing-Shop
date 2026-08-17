using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Users;

namespace Application.Features.Users.Mapper;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(
                dest => dest.UserId,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(
                dest => dest.PhoneNumber,
                opt => opt.MapFrom(src => src.PhoneNumber.Value))
            .ForMember(
                dest => dest.IsEmailVerified,
                opt => opt.MapFrom(src => src.EmailVerified))
            .ForMember(
                dest => dest.Role,
                opt => opt.MapFrom(src => src.UserRole.ToString()));
    }
}
