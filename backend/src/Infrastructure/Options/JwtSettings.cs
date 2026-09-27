using System.ComponentModel.DataAnnotations;

namespace infrastructure.Options;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required(ErrorMessage = "JWT secret key is required.")]
    [MinLength(32, ErrorMessage = "JWT secret key must be at least 32 characters (256 bits) long.")]
    public string SecretKey { get; init; } = string.Empty;

    [Range(1, 43200, ErrorMessage = "Token expiration must be between 1 minute and 30 days.")]
    public int TokenExpirationInMinutes { get; init; }

    [Required(ErrorMessage = "JWT issuer is required.")]
    public string Issuer { get; init; } = string.Empty;

    [Required(ErrorMessage = "JWT audience is required.")]
    public string Audience { get; init; } = string.Empty;
}
