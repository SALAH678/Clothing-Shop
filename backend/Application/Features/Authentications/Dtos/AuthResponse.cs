using Application.Features.Identity;
using Application.Features.Users.Dtos;

namespace Application.Features.Authentications.Dtos;

public record AuthResponse(
        UserDto User,
        TokenResponse Tokens);
