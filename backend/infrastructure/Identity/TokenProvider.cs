using Application.Common.Interfaces;
using Application.Features.Identity;
using Domain.Common.Results;
using infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace infrastructure.Identity;

public class TokenProvider(IOptions<JwtSettings> jwtSettingsOptions) : ITokenProvider
{
    private readonly JwtSettings _jwtSettings = jwtSettingsOptions.Value;

    public Result<TokenResponse> GenerateJwtToken(string userId, string email, string role)
    {

        var tokenResutl = Create(userId, email, role);

        if (!tokenResutl.IsSuccess)
            return tokenResutl.TopError;

        return tokenResutl.Value;
    }

    private Result<TokenResponse> Create(string userId, string email, string role)
    {
        var expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
        };

        var accessToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: creds
        );

        var refreshToken = GenerateRefreshToken();

        return new TokenResponse(
            AccessToken: new JwtSecurityTokenHandler().WriteToken(accessToken),
            RefreshToken: refreshToken,
            TokenType: "Bearer",
            AccessTokenExpiration: expires,
            RefreshTokenExpiresAtUtc: default
        );
    }

    private static string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
