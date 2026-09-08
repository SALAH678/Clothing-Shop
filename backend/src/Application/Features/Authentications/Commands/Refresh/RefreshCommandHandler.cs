using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Features.Authentications.Dtos;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using Domain.Users.RefreshTokens;
using MediatR;
using Microsoft.Extensions.Logging;
namespace Application.Features.Authentications.Command.Refresh;

public class RefreshCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ITokenHasherService tokenHasherService,
        ITokenProvider tokenProvider, ILogger<RefreshCommandHandler> logger) : IRequestHandler<RefreshCommand, Result<AuthResponse>>
{
    private readonly ILogger<RefreshCommandHandler> _logger = logger;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenHasherService _tokenHasherService = tokenHasherService;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    public async Task<Result<AuthResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token refresh attempt");

        var requestedRefreshTokenHash = _tokenHasherService.HashToken(request.refreshToken);

        var refreshTokenInDb = await _unitOfWork.RefreshTokens.GetByValueAsync(
            value: requestedRefreshTokenHash,
            cancellationToken: cancellationToken);

        if (refreshTokenInDb is null)
        {
            _logger.LogWarning("Refresh failed: refresh token not found");
            return ApplicationErrors.RefreshTokenNotFound;
        }

        if (refreshTokenInDb.IsRevoked)
        {
            _logger.LogWarning("Refresh failed: refresh token is revoked for UserId: {UserId}", refreshTokenInDb.UserId);
            return ApplicationErrors.RefreshTokenIsRevoked;
        }

        if (refreshTokenInDb.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            _logger.LogWarning("Refresh failed: refresh token expired for UserId: {UserId}", refreshTokenInDb.UserId);
            return ApplicationErrors.RefreshTokenExpired;
        }

        var user = await _unitOfWork.Users.GetByIdAsync(refreshTokenInDb.UserId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Refresh failed: user not found for UserId: {UserId}", refreshTokenInDb.UserId);
            return ApplicationErrors.InvalidRefreshRequest;
        }

        refreshTokenInDb.Revoke();

        var tokensResult = _tokenProvider.GenerateJwtToken(
           user.Id.ToString(),
           user.Email.Value,
           user.UserRole.ToString());

        if (!tokensResult.IsSuccess)
        { 
            _logger.LogError("Refresh failed: unable to generate tokens for UserId: {UserId}", user.Id);
            return tokensResult.TopError;
        }

        var refreshTokenExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(7);
        var finalTokens = tokensResult.Value with { RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc };

        if (finalTokens.RefreshToken is null)
        {
            _logger.LogError("Refresh failed: generated refresh token is null for UserId: {UserId}", user.Id);
            return ApplicationErrors.RefreshFailed;
        }

        var newHashedRefreshToken = _tokenHasherService.HashToken(finalTokens.RefreshToken);

        var refreshTokenResult = RefreshToken.Create(
            user.Id,
            newHashedRefreshToken,
            refreshTokenExpiresAtUtc);

        if (!refreshTokenResult.IsSuccess)
        {
            _logger.LogError("Refresh failed: unable to create new refresh token for UserId: {UserId}", user.Id);
            return refreshTokenResult.TopError;
        }

        _unitOfWork.RefreshTokens.Create(refreshTokenResult.Value);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh failed: unable to save new refresh token for Email: {Email}, UserId: {UserId}", user.Email.Value, user.Id);
            return ApplicationErrors.RefreshFailed;
        }

        _logger.LogInformation("Token refresh successful for Email: {Email}, UserId: {UserId}", user.Email.Value, user.Id);

        return new AuthResponse(
            User: _mapper.Map<UserDto>(user),
            Tokens: finalTokens);
    }
}
