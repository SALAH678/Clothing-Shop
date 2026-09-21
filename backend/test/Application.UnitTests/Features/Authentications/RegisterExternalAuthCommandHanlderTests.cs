using Application.Common.Errors;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Features.Authentications.Commands.RegisterExternalAuth;
using AutoMapper;
using Domain.Common.ValueObjects.Email;
using Domain.Users;
using Domain.Users.Accounts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.UnitTests.Features.Authentications;

public class RegisterExternalAuthCommandHanlderTests
{
    private readonly Mock<IOAuthService> _oAuthServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITokenProvider> _tokenProviderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ITokenHasherService> _tokenHasherServiceMock;
    private readonly ILogger<RegisterExternalAuthCommandHandler> _loggerMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IAccountRepository> _accountRepoMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly RegisterExternalAuthCommandHandler _handler;

    public RegisterExternalAuthCommandHanlderTests()
    {
        _oAuthServiceMock = new Mock<IOAuthService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tokenProviderMock = new Mock<ITokenProvider>();
        _mapperMock = new Mock<IMapper>();
        _tokenHasherServiceMock = new Mock<ITokenHasherService>();
        _userRepoMock = new Mock<IUserRepository>();
        _accountRepoMock = new Mock<IAccountRepository>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _loggerMock = NullLogger<RegisterExternalAuthCommandHandler>.Instance;

        _unitOfWorkMock.SetupGet(u => u.Users).Returns(_userRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Accounts).Returns(_accountRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.RefreshTokens).Returns(_refreshTokenRepoMock.Object);

        _handler = new RegisterExternalAuthCommandHandler(
            _oAuthServiceMock.Object,
            _unitOfWorkMock.Object,
            _tokenProviderMock.Object,
            _tokenHasherServiceMock.Object,
            _mapperMock.Object,
            _loggerMock);

    }

    private static OAuthUserInfo ValidGoogleInfo(string email = "test@gmail.com") =>
        new(ProviderAccountId: "google-123", Email: email, FirstName: "John",
            LastName: "Doe", EmailVerified: true);

    [Fact]
    public async Task Handle_Should_ReturnInvalidGoogleId_When_GoogleTokenIsInvalid()
    {
        //Arrange
        var command = new RegisterExternalAuthCommand("invalid_token", "1234567890");

        _oAuthServiceMock.Setup(
            x => x.ValidateGoogleTokenAsync(command.IdToken, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOAuthTokenException("Google ID token validation failed.", default!));

        //Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        //Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(ApplicationErrors.InvalidGoogleId);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmailNotVerified_When_EmailIsNotVerified()
    {
        //Arrange
        var command = new RegisterExternalAuthCommand("valid_token", "1234567890");
        var unverifiedInfo = ValidGoogleInfo() with { EmailVerified = false };

        _oAuthServiceMock.Setup(
            x => x.ValidateGoogleTokenAsync(command.IdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unverifiedInfo);

        //Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        //Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(ApplicationErrors.EmailNotVerified);
        _accountRepoMock.Verify(
            a => a.GetByProviderAsync(unverifiedInfo.ProviderAccountId, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_AccountAlreadyExists_When_AccountWithSameProviderIdExists()
    {
        //Arrange
        var command = new RegisterExternalAuthCommand("valid_token", "1234567890");
        var validInfo = ValidGoogleInfo();
        var dummyAccount = Account.Create(Guid.NewGuid(), "Google", validInfo.ProviderAccountId).Value;

        _oAuthServiceMock.Setup(
            x => x.ValidateGoogleTokenAsync(command.IdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validInfo);

        _accountRepoMock.Setup(
            a => a.GetByProviderAsync(validInfo.ProviderAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dummyAccount);
        //Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        //Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(ApplicationErrors.AccountAlreadyExists);
    }

    [Fact]
    public async Task Hanlde_Should_ReturnEmailError_When_EmailCreationFailed()
    {
        //Arrange
        var command = new RegisterExternalAuthCommand("valid_token", "1234567890");
        var validInfo = ValidGoogleInfo("sdfs.com");

        _oAuthServiceMock.Setup(
            x => x.ValidateGoogleTokenAsync(command.IdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validInfo);

        _accountRepoMock.Setup(
            a => a.GetByProviderAsync(validInfo.ProviderAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        //Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        //Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().NotBeNull();
        result.TopError.Should().Be(EmailErrors.InvalidEmail);
    }
}
