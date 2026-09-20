using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Authentications.Command.LogOut;

public class LogOutCommandHandler(IUnitOfWork unitOfWork,
       ITokenHasherService tokenHasherService, ILogger<LogOutCommandHandler> logger) : IRequestHandler<LogOutCommand, Result<Success>>
{
    private readonly ILogger<LogOutCommandHandler> _logger = logger;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITokenHasherService _tokenHasherService = tokenHasherService;

    public async Task<Result<Success>> Handle(LogOutCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logout attempt");

        var requestedRefreshTokenHash = _tokenHasherService.HashToken(request.refreshToken);

        var refreshTokenInDb = await _unitOfWork.RefreshTokens.GetByValueAsync(
            value: requestedRefreshTokenHash,
            cancellationToken: cancellationToken);

        if (refreshTokenInDb is null)
        {
            _logger.LogInformation("Logout: refresh token not found, treating as success.");
            return Result.Success;// Treating as success to avoid revealing token validity
        }

        if (refreshTokenInDb.IsRevoked)
        {
            _logger.LogInformation("Logout: refresh token already revoked for UserId: {UserId}", refreshTokenInDb.UserId);
            return Result.Success;
        }

        refreshTokenInDb.Revoke();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Logout successful for UserId: {UserId}", refreshTokenInDb.UserId);

        return Result.Success;
    }
}
