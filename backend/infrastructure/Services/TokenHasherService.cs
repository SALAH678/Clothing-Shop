using Application.Common.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

namespace infrastructure.Services;

public sealed class TokenHasherService : ITokenHasherService
{
    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    public bool VerifyRefreshToken(string token, string hashedToken) =>
        HashToken(token) == hashedToken;
}
