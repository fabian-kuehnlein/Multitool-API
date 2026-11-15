using System.Security.Authentication;
using Mapster;
using Moq;
using Multitool.Application.Mappings;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtMock;

    private User? _getUserByUsernameResponse;
    private bool _verifyPasswordResponse;
    private string _generateTokenResponse;

    private static readonly string USERNAME = AuthTestData.DefaultUser.Username;
    private static readonly string PASSWORD = AuthTestData.DefaultLoginRequest.Password;
    private const string TOKEN = "token-123";

    public AuthenticationServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());

        _userRepositoryMock = new Mock<IUserRepository>();
        _hasherMock = new Mock<IPasswordHasher>();
        _jwtMock = new Mock<IJwtTokenGenerator>();

        _getUserByUsernameResponse = null;
        _verifyPasswordResponse = true;
        _generateTokenResponse = TOKEN;
    }

    private AuthenticationService GetService()
    {
        _userRepositoryMock.Reset();
        _hasherMock.Reset();
        _jwtMock.Reset();

        _userRepositoryMock.Setup(r => r.GetUserByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(_getUserByUsernameResponse);

        _userRepositoryMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _hasherMock.Setup(h => h.Hash(It.IsAny<string>()))
            .Returns(AuthTestData.DefaultUser.PasswordHash);

        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(_verifyPasswordResponse);

        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns(_generateTokenResponse);

        return new AuthenticationService(
            _userRepositoryMock.Object,
            _hasherMock.Object,
            _jwtMock.Object);
    }

    // LoginAsync
    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsTokenAndResetsLockoutState()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.AccessFailedCount = 3;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(-5); // In the past

        _getUserByUsernameResponse = user;
        var service = GetService();

        // Act
        var result = await service.LoginAsync(USERNAME, PASSWORD);

        // Assert
        AssertEx.AreEqual(TOKEN, result);
        AssertEx.AreEqual(0, user.AccessFailedCount);
        AssertEx.AreEqual(null, user.LockoutEnd);

        _hasherMock.Verify(h => h.Verify(PASSWORD, user.PasswordHash), Times.Once);
        _hasherMock.VerifyNoOtherCalls();

        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(USERNAME), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.Is<User>(u =>
            u.Username == USERNAME &&
            u.AccessFailedCount == 0 &&
            u.LockoutEnd == null
        )), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(user), Times.Once);
        _jwtMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsInactive_ThrowsInvalidCredentialException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.IsActive = false;

        _getUserByUsernameResponse = user;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.LoginAsync(USERNAME, PASSWORD);

        // Assert
        await AssertEx.Throws<InvalidCredentialException>(act);

        _hasherMock.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _hasherMock.VerifyNoOtherCalls();

        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(USERNAME), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _jwtMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsLockedOut_ThrowsInvalidCredentialException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);

        _getUserByUsernameResponse = user;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.LoginAsync(USERNAME, "wrong-password");

        // Assert
        await AssertEx.Throws<InvalidCredentialException>(act);

        _hasherMock.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _hasherMock.VerifyNoOtherCalls();

        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(USERNAME), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _jwtMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_IncrementsFailureCount()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.AccessFailedCount = 0;

        _getUserByUsernameResponse = user;
        _verifyPasswordResponse = false;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.LoginAsync(USERNAME, "wrong-password");

        // Assert
        await AssertEx.Throws<InvalidCredentialException>(act);
        AssertEx.AreEqual(1, user.AccessFailedCount);

        _hasherMock.Verify(h => h.Verify("wrong-password", user.PasswordHash), Times.Once);
        _hasherMock.VerifyNoOtherCalls();

        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(USERNAME), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.Is<User>(u =>
            u.Username == USERNAME &&
            u.AccessFailedCount == 1
        )), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoginAsync_WhenFailureThresholdIsReached_SetsLockoutEnd()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.AccessFailedCount = 4;

        _getUserByUsernameResponse = user;
        _verifyPasswordResponse = false;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.LoginAsync(USERNAME, "wrong-password");

        // Assert
        await AssertEx.Throws<InvalidCredentialException>(act);
        AssertEx.AreEqual(5, user.AccessFailedCount);
        AssertEx.CloseTo(user.LockoutEnd!.Value, DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));

        _hasherMock.Verify(h => h.Verify("wrong-password", user.PasswordHash), Times.Once);
        _hasherMock.VerifyNoOtherCalls();

        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(USERNAME), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.Is<User>(u =>
            u.Username == USERNAME &&
            u.AccessFailedCount == 5 &&
            u.LockoutEnd != null
        )), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoginAsync_WhenUsernameDoesNotExist_ThrowsInvalidCredentialException()
    {
        // Arrange
        _getUserByUsernameResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.LoginAsync("unknown", PASSWORD);

        // Assert
        await AssertEx.Throws<InvalidCredentialException>(act);

        _hasherMock.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _hasherMock.VerifyNoOtherCalls();

        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync("unknown"), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _jwtMock.VerifyNoOtherCalls();
    }
}
