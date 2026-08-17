
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Users.Query.GetCurrentUserProfile;

public class GetCurrentUserProfileQueryHandler(IUser user,
    IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetCurrentUserProfileQuery, Result<UserDto>>
{
    private readonly IUser _user = user;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<UserDto>> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(_user.UserId, cancellationToken);

        if (user is null)
            return ApplicationErrors.UserNotFound;

        var userDto = _mapper.Map<UserDto>(user);

        return userDto;
    }
}
