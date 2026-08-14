namespace Application.Common.Interfaces.Services;

public interface ITokenHasherService
{
    string HashToken(string token);
    bool VerifyRefreshToken(string token, string hashedToken);
}
