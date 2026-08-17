using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
using Domain.Common.ValueObjects.PhoneNumber;
using MediatR;

namespace Application.Features.Users.Command.UpdateCurrentUserProfile;

public class UpdateCurrentUserProfileCommandHandler(IUnitOfWork unitOfWork,
    IMapper mapper, IUser user) : IRequestHandler<UpdateCurrentUserProfileCommand, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IUser _user = user;

    public async Task<Result<UserDto>> Handle(UpdateCurrentUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(_user.UserId, cancellationToken);

        if (user is null)
            return ApplicationErrors.UserNotFound;

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
            return updatedUser.TopError;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            return ApplicationErrors.UpdateUserFailed;
        }

        return _mapper.Map<UserDto>(user);
    }
}
