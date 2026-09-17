using Application.Features.Users.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Users.Command.CreateUser;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Email,
    string Password,
    string? Role = "Customer") : IRequest<Result<UserDto>>;
