using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Features.Authentications.Dtos;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Authentications.Command.ExternalAuthentication;

public class ExternalAuthHandler(IExternalAuthenticator externalAuthenticator, IAuthenticationTokenService authenticationTokenService, IUnitOfWork unitOfWork,
    IMapper mapper, ILogger<ExternalAuthHandler> logger) : IRequestHandler<ExternalAuthCommand, Result<AuthResponse>>
{
    private readonly IExternalAuthenticator _externalAuthenticator = externalAuthenticator;
    private readonly IAuthenticationTokenService _authenticationTokenService = authenticationTokenService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<ExternalAuthHandler> _logger = logger;

    public async Task<Result<AuthResponse>> Handle(ExternalAuthCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("External authentication attempt started");

        // 1. Authenticate with Google
        //    -> validate Google token
        //    -> find existing Google account
        //    -> find existing user
        //    -> create user if necessary
        //    -> link Google account
        var userResult = await _externalAuthenticator.AuthenticateAsync( request.IdToken, request.phoneNumber, cancellationToken);

        if (userResult.IsError)
        {
            return userResult.Errors;
        }

        var user = userResult.Value;

        // 2. Generate access token + refresh token
        var tokenResult = await _authenticationTokenService.CreateAsync(user, cancellationToken);

        if (tokenResult.IsError)
        {
            return tokenResult.Errors;
        }

        // 3. Persist User/Account/RefreshToken changes
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving changes during external authentication for UserId {UserId}", user.Id);

            return ApplicationErrors.OAuthFailed;
        }

        _logger.LogInformation("External authentication succeeded for UserId {UserId}", user.Id);

        // 4. Return response
        return new AuthResponse(
            User: _mapper.Map<UserDto>(user),
            Tokens: tokenResult.Value);
    }
}