using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Features.Authentications.Dtos;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
using Domain.Users.RefreshTokens;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Authentications.Command.Refresh;

public class RefreshCommandHandler(IUnitOfWork unitOfWork,
        IMapper mapper, ITokenHasherService tokenHasherService,
        ITokenProvider tokenProvider, ILogger<RefreshCommandHandler> logger) : IRequestHandler<RefreshCommand, Result<AuthResponse>>
{
    private readonly ILogger<RefreshCommandHandler> _logger = logger;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenHasherService _tokenHasherService = tokenHasherService;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    public async Task<Result<AuthResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token refresh attempt for Email: {Email}", request.email);

        var requestedRefreshTokenHash = _tokenHasherService.HashToken(request.refreshToken);

        var refreshTokenInDb = await _unitOfWork.RefreshTokens.GetByValueAsync(
            value: requestedRefreshTokenHash,
            cancellationToken: cancellationToken);

        if (refreshTokenInDb is null)
        {
            _logger.LogWarning("Refresh failed: refresh token not found for Email: {Email}", request.email);
            return ApplicationErrors.RefreshTokenNotFound;
        }

        if (refreshTokenInDb.IsRevoked)
        {
            _logger.LogWarning("Refresh failed: refresh token is revoked for Email: {Email}, UserId: {UserId}", request.email, refreshTokenInDb.UserId);
            return ApplicationErrors.RefreshTokenIsRevoked;
        }

        if (refreshTokenInDb.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            _logger.LogWarning("Refresh failed: refresh token expired for Email: {Email}, UserId: {UserId}", request.email, refreshTokenInDb.UserId);
            return ApplicationErrors.RefreshTokenExpired;
        }

        var emailResult = Email.Create(request.email);

        var user = await _unitOfWork.Users.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (user is null)
            return ApplicationErrors.InvalidRefreshRequest;

        refreshTokenInDb.Revoke();

        var tokensResult = _tokenProvider.GenerateJwtToken(
           user.Id.ToString(),
           user.Email.Value,
           user.UserRole.ToString());

        if (!tokensResult.IsSuccess)
            return tokensResult.TopError;

        var newHashedRefreshToken = _tokenHasherService.HashToken(tokensResult.Value.RefreshToken!);

        var refreshTokenResult = RefreshToken.Create(
            user.Id,
            newHashedRefreshToken,
            DateTimeOffset.UtcNow.AddDays(7));

        if (!refreshTokenResult.IsSuccess)
            return refreshTokenResult.TopError;

        _unitOfWork.RefreshTokens.Create(refreshTokenResult.Value);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh failed: unable to save new refresh token for Email: {Email}, UserId: {UserId}", request.email, user.Id);
            return ApplicationErrors.RefreshFailed;
        }

        _logger.LogInformation("Token refresh successful for Email: {Email}, UserId: {UserId}", request.email, user.Id);

        return new AuthResponse(
            User: _mapper.Map<UserDto>(user),
            Tokens: tokensResult.Value);
    }
}
