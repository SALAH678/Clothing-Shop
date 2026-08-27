using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
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
        _logger.LogInformation("Logout attempt for Email: {Email}", request.email);

        var emailResult = Email.Create(request.email);
        var user = await _unitOfWork.Users.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Logout failed: user not found for Email: {Email}", request.email);
            return ApplicationErrors.InvalidRefreshRequest;
        }

        var requestedRefreshTokenHash = _tokenHasherService.HashToken(request.refreshToken);

        var refreshTokenInDb = await _unitOfWork.RefreshTokens.GetByValueAsync(
            value: requestedRefreshTokenHash,
            cancellationToken: cancellationToken);

        if (refreshTokenInDb is null)
            return ApplicationErrors.RefreshTokenNotFound;

        if (refreshTokenInDb.UserId != user.Id)
            return ApplicationErrors.InvalidRefreshRequest;

        if (refreshTokenInDb.IsRevoked)
            return ApplicationErrors.RefreshTokenIsRevoked;

        if (refreshTokenInDb.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            return ApplicationErrors.RefreshTokenExpired;

        refreshTokenInDb.Revoke();

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed: unable to revoke refresh token for Email: {Email}, UserId: {UserId}", request.email, user.Id);
            return ApplicationErrors.LogOutFailed;
        }

        _logger.LogInformation("Logout successful for Email: {Email}, UserId: {UserId}", request.email, user.Id);

        return Result.Success;
    }
}
