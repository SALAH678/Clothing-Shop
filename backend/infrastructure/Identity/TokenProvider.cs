using Application.Common.Interfaces;
using Application.Features.Identity;
using Domain.Common.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace infrastructure.Identity;

public class TokenProvider(IConfiguration configuration) : ITokenProvider
{
    private readonly IConfiguration _configuration = configuration;

    public Result<TokenResponse> GenerateJwtToken(string userId, string email, string role)
    {

        var tokenResutl = Create(userId, email, role);

        if (!tokenResutl.IsSuccess)
            return tokenResutl.TopError;

        return tokenResutl.Value;
    }

    private Result<TokenResponse> Create(string userId, string email, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var secretkey = jwtSettings["SecretKey"]!;
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["TokenExpirationInMinutes"]!));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

        var accessToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var refreshToken = GenerateRefreshToken();

        return new TokenResponse(
            AccessToken: new JwtSecurityTokenHandler().WriteToken(accessToken),
            RefreshToken: refreshToken,
            TokenType: "Bearer",
            AccessTokenExpiration: expires
        );
    }

    private static string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
