using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
using Domain.Common.ValueObjects.PhoneNumber;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Users.Command.UpdateCurrentUserProfile;

public class UpdateCurrentUserProfileCommandHandler(IUnitOfWork unitOfWork,
    IMapper mapper, IUser user, ILogger<UpdateCurrentUserProfileCommandHandler> logger) : IRequestHandler<UpdateCurrentUserProfileCommand, Result<UserDto>>
{
    private readonly ILogger<UpdateCurrentUserProfileCommandHandler> _logger = logger;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IUser _user = user;

    public async Task<Result<UserDto>> Handle(UpdateCurrentUserProfileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Update current user profile requested for Email: {Email}, UserId: {UserId}", _user.Email, _user.UserId);

        var user = await _unitOfWork.Users.GetByIdAsync(_user.UserId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Update current user profile failed: user not found for Email: {Email}, UserId: {UserId}", _user.Email, _user.UserId);
            return ApplicationErrors.UserNotFound;
        }

        Result<PhoneNumber>? newNumber = null;

        if (request.PhoneNumber is not null && request.PhoneNumber != user.PhoneNumber.Value)
        {
            newNumber = PhoneNumber.Create(request.PhoneNumber);

            if (!newNumber.IsSuccess)
                return newNumber.TopError;
        }

        var updatedUser = user.Update(
            firstName: request.FirstName ?? user.FirstName,
            lastName: request.LastName ?? user.LastName,
            email: user.Email,
            phoneNumber: newNumber?.Value ?? user.PhoneNumber
        );

        if (!updatedUser.IsSuccess)
        {
            _logger.LogWarning("Update current user profile failed for Email: {Email}, UserId: {UserId}", _user.Email, _user.UserId);
            return updatedUser.TopError;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Current user profile updated successfully for Email: {Email}, UserId: {UserId}", _user.Email, _user.UserId);

        return _mapper.Map<UserDto>(user);
    }
}
