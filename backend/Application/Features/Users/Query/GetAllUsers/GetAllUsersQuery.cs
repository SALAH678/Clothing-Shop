using Application.Features.Users.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Users.Query.GetAllUsers;

public sealed record GetAllUsersQuery : IRequest<Result<List<UserDto>>>;