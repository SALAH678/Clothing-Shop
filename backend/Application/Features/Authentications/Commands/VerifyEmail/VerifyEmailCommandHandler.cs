using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Features.Authentications.Dtos;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
using Domain.Users.RefreshTokens;
using Domain.Users.VerificationTokens.Enum;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Authentications.Command.VerifyEmail;

public sealed class VerifyEmailCommandHandler(IUnitOfWork unitOfWork,
        ITokenProvider tokenProvider, IMapper mapper, ITokenHasherService tokenHasher, ILogger<VerifyEmailCommandHandler> logger) : IRequestHandler<VerifyEmailCommand, Result<AuthResponse>>
{
    private readonly ILogger<VerifyEmailCommandHandler> _logger = logger;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenHasherService _tokenHasher = tokenHasher;

    public async Task<Result<AuthResponse>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email verification attempt for Email: {Email}", request.Email);

        var emailResult = Email.Create(request.Email);

        var user = await _unitOfWork.Users
            .GetByEmailWithTrackingAsync(emailResult.Value, cancellationToken);

        if (user is null)
            return ApplicationErrors.InvalidCredentials;

        if (user.EmailVerified)
        {
            _logger.LogWarning("VerifyEmail: email already verified for Email: {Email}, UserId: {UserId}", request.Email, user.Id);
            return ApplicationErrors.EmailAlreadyVerified;
        }

        var verificationToken = await _unitOfWork.VerificationTokens
            .GetByUserIdAsync(user.Id, VerificationTokenType.EmailVerification, cancellationToken);

        if (verificationToken is null)
            return ApplicationErrors.VerificationTokenNotFound;

        var verifyCode = verificationToken.Verify(request.Code);

        if (!verifyCode.IsSuccess)
            return verifyCode.TopError;

        var verificationTokenUsed = verificationToken.MarkAsUsed();

        if(!verificationTokenUsed.IsSuccess)
            return verificationTokenUsed.TopError;

        user.MarkEmailVerified();

        var tokens = _tokenProvider.GenerateJwtToken(
            user.Id.ToString(),
            user.Email.Value,
            user.UserRole.ToString());

        if (!tokens.IsSuccess)
            return tokens.TopError;

        var hashedToken = _tokenHasher.HashToken(tokens.Value.RefreshToken!);

        var newRefreshToken = RefreshToken.Create(
            userId: user.Id,
            value: hashedToken,
            expiresAtUtc: DateTimeOffset.UtcNow.AddDays(7));

        if (!newRefreshToken.IsSuccess)
            return newRefreshToken.TopError;

        _unitOfWork.RefreshTokens.Create(newRefreshToken.Value);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VerifyEmail failed: unable to save refresh token after verification for Email: {Email}", request.Email);
            return ApplicationErrors.VerificationFailed;
        }

        _logger.LogInformation("Email verification successful for Email: {Email}, UserId: {UserId}", request.Email, user.Id);

        return new AuthResponse(
            User: _mapper.Map<UserDto>(user),
            Tokens: tokens.Value);
    }
}
