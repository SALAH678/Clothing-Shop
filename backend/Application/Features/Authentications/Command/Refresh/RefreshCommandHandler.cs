using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Features.Authentications.Dtos;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using Domain.Users.RefreshTokens;
using MediatR;

namespace Application.Features.Authentications.Command.Refresh;

public class RefreshCommandHandler(IUnitOfWork unitOfWork,
        IMapper mapper, ITokenHasherService tokenHasherService,
        ITokenProvider tokenProvider) : IRequestHandler<RefreshCommand, Result<AuthResponse>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenHasherService _tokenHasherService = tokenHasherService;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    public async Task<Result<AuthResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var requestedRefreshTokenHash = _tokenHasherService.HashToken(request.refreshToken);

        var refreshTokenInDb = await _unitOfWork.RefreshTokens.GetByValueAsync(
            value: requestedRefreshTokenHash,
            cancellationToken: cancellationToken);

        if (refreshTokenInDb is null)
            return ApplicationErrors.RefreshTokenNotFound;

        if (refreshTokenInDb.IsRevoked)
            return ApplicationErrors.RefreshTokenIsRevoked;

        if (refreshTokenInDb.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            return ApplicationErrors.RefreshTokenExpired;

        var user = await _unitOfWork.Users.GetByEmailAsync(request.email, cancellationToken);

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
        catch
        {
            return ApplicationErrors.RefreshFailed;
        }

        return new AuthResponse(
            User: _mapper.Map<UserDto>(user),
            Tokens: tokensResult.Value);
    }
}
