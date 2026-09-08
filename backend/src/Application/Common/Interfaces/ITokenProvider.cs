using Application.Features.Identity;
using Domain.Common.Results;

namespace Application.Common.Interfaces;
public interface ITokenProvider
{
    Result<TokenResponse> GenerateJwtToken(string UserId, string email, string role);
}
