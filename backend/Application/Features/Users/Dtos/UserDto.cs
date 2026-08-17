namespace Application.Features.Users.Dtos;

public record UserDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string PhoneNumber,
    bool IsEmailVerified);
