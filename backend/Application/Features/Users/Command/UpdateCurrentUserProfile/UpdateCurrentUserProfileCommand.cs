using Application.Features.Users.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Users.Command.UpdateCurrentUserProfile;

public sealed record UpdateCurrentUserProfileCommand(
    string? FirstName,
    string? LastName,
    string? PhoneNumber) : IRequest<Result<UserDto>>;  
