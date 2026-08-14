namespace Application.Features.Identity;
public record TokenResponse(
        string? AccessToken,
        string? RefreshToken,
        string? TokenType,
        DateTime AccessTokenExpiration
    );
