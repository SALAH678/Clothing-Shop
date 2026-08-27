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
namespace Application.Features.Authentications.Command.Login;

public class LoginCommandHandler(IUnitOfWork unitOfWork,
        IMapper mapper, IPasswordService passwordService,
        ITokenProvider tokenProvider, ITokenHasherService tokenHasher, ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly ILogger<LoginCommandHandler> _logger = logger;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly ITokenHasherService _tokenHasher = tokenHasher;

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for Email: {Email}", request.Email);

        var emailResult = Email.Create(request.Email);

        var user = await _unitOfWork.Users.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found for Email: {Email}", request.Email);
            return ApplicationErrors.InvalidCredentials;
        }

        if (!user.EmailVerified)
        {
            _logger.LogWarning("Login failed: email not verified for Email: {Email}, UserId: {UserId}", request.Email, user.Id);
            return ApplicationErrors.EmailNotVerified;
        }

        var account = await _unitOfWork.Accounts.GetByUserIdAsync(user.Id, cancellationToken);

        if(account is null)
            return ApplicationErrors.InvalidCredentials;

        if (!_passwordService.VerifyPassword(request.Password, account.Password!.Value))
        {
            _logger.LogWarning("Login failed: invalid password for Email: {Email}, UserId: {UserId}", request.Email, user.Id);
            return ApplicationErrors.InvalidCredentials;
        }

        var tokens = _tokenProvider.GenerateJwtToken(user.Id.ToString(), user.Email.Value, user.UserRole.ToString());

        if (!tokens.IsSuccess)
            return tokens.TopError;

        var hashedToken = _tokenHasher.HashToken(tokens.Value.RefreshToken!);

        var newRefreshToken = RefreshToken.Create(
            userId: account.UserId,
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
            _logger.LogError(ex, "Login failed: unable to save refresh token for Email: {Email}, UserId: {UserId}", request.Email, user.Id);
            return ApplicationErrors.LoginFailed;
        }

        _logger.LogInformation("Login successful for Email: {Email}, UserId: {UserId}", request.Email, user.Id);

        return new AuthResponse(
            User: _mapper.Map<UserDto>(user),
            Tokens: tokens.Value);
    }
}
