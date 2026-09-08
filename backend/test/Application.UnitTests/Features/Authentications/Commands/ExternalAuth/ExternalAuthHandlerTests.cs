using Application.Common.Errors;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Features.Authentications.Command.ExternalAuthentication;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.UnitTests.Features.Authentications.Commands.ExternalAuth;

public class ExternalAuthHandlerTests
{
    private readonly Mock<IOAuthService> _oAuthServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITokenProvider> _tokenProviderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ITokenHasherService> _tokenHasherServiceMock;
    private readonly ILogger<ExternalAuthHandler> _loggerMock;

    public ExternalAuthHandlerTests()
    {
        _oAuthServiceMock = new Mock<IOAuthService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tokenProviderMock = new Mock<ITokenProvider>();
        _mapperMock = new Mock<IMapper>();
        _tokenHasherServiceMock = new Mock<ITokenHasherService>();
        _loggerMock = NullLogger<ExternalAuthHandler>.Instance;
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalidOAuthTokenException_When_GoogleTokenIsInvalid()
    {
        //Arrange
        var command = new ExternalAuthCommand("invalid_token", "1234567890");

        _oAuthServiceMock.Setup(
            x => x.ValidateGoogleTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOAuthTokenException("Google ID token validation failed.", default!));

        var handler = new ExternalAuthHandler(_oAuthServiceMock.Object, _unitOfWorkMock.Object, _tokenProviderMock.Object,
            _mapperMock.Object, _tokenHasherServiceMock.Object, _loggerMock);

        //Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        //Assert
        result.TopError.Should().Be(ApplicationErrors.InvalidGoogleId);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmailNotVerifiedApplicationError_When_GoogleEmailIsNotVerified()
    {
        //Arrange
        var command = new ExternalAuthCommand("invalid_token", "1234567890");

        _oAuthServiceMock.Setup(
            x => x.ValidateGoogleTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OAuthUserInfo("test@example.com", false, string.Empty,
                string.Empty, string.Empty));

        var handler = new ExternalAuthHandler(_oAuthServiceMock.Object, _unitOfWorkMock.Object, _tokenProviderMock.Object,
            _mapperMock.Object, _tokenHasherServiceMock.Object, _loggerMock);

        //Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        //Assert
        result.TopError.Should().Be(ApplicationErrors.EmailNotVerified);
    }
}
