
using Application.Features.Users.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Users.Query.GetCurrentUserProfile;

public sealed record GetCurrentUserProfileQuery : IRequest<Result<UserDto>>;
