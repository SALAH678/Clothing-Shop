using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.LogOut;

public class LogOutCommandHandler(IUnitOfWork unitOfWork,
       ITokenHasherService tokenHasherService) : IRequestHandler<LogOutCommand, Result<Success>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITokenHasherService _tokenHasherService = tokenHasherService;

    public async Task<Result<Success>> Handle(LogOutCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.email, cancellationToken);

        if (user is null)
            return ApplicationErrors.InvalidRefreshRequest;

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
        catch
        {
            return ApplicationErrors.LogOutFailed;
        }

        return Result.Success;
    }
}
