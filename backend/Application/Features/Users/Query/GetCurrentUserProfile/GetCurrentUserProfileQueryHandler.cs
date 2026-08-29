
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Users.Query.GetCurrentUserProfile;

public class GetCurrentUserProfileQueryHandler(IUser user,
    IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetCurrentUserProfileQueryHandler> logger) : IRequestHandler<GetCurrentUserProfileQuery, Result<UserDto>>
{
    private readonly ILogger<GetCurrentUserProfileQueryHandler> _logger = logger;

    private readonly IUser _user = user;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<UserDto>> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get current user profile requested for Email: {Email}, UserId: {UserId}", _user.Email, _user.UserId);

        var user = await _unitOfWork.Users.GetByIdAsync(_user.UserId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Get current user profile failed: user not found for Email: {Email}, UserId: {UserId}", _user.Email, _user.UserId);
            return ApplicationErrors.UserNotFound;
        }

        var userDto = _mapper.Map<UserDto>(user);

        return userDto;
    }
}
