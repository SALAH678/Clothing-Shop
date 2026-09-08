namespace Application.Features.Identity;
public record TokenResponse(
        string? AccessToken,
        string? RefreshToken,
        string? TokenType,
        DateTimeOffset AccessTokenExpiration,
        DateTimeOffset RefreshTokenExpiresAtUtc
);
