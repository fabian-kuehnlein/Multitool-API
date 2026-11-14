using System.Security.Authentication;
using Mapster;
using Moq;
using Multitool.Application.Mappings;
using Multitool.Application.Models.Account;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class AccountServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtMock;
    private readonly Mock<IUserContext> _userContextMock;

    private User? _getUserByIdResponse;
    private User? _getUserByUsernameResponse;
    private string _hashedPasswordResponse;
    private string _generateTokenResponse;

    private static readonly int USER_ID = AuthTestData.DefaultUser.Id;
    private static readonly int OTHER_USER_ID = AuthTestData.DefaultUser.Id + 1;
    private const string HASHED_PASSWORD = "hashed-new-password";
    private const string TOKEN = "token-456";

    private User? _updatedUser;

    public AccountServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());

        _userRepositoryMock = new Mock<IUserRepository>();
        _hasherMock = new Mock<IPasswordHasher>();
        _jwtMock = new Mock<IJwtTokenGenerator>();
        _userContextMock = new Mock<IUserContext>();
        _userContextMock.SetupGet(c => c.UserId).Returns(USER_ID);

        _getUserByIdResponse = AuthTestData.DefaultUser;
        _getUserByUsernameResponse = null;
        _hashedPasswordResponse = HASHED_PASSWORD;
        _generateTokenResponse = TOKEN;
    }

    private AccountService GetService()
    {
        _updatedUser = null;

        _userRepositoryMock.Reset();
        _hasherMock.Reset();
        _jwtMock.Reset();

        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(_getUserByIdResponse);

        _userRepositoryMock.Setup(r => r.GetUserByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(_getUserByUsernameResponse);

        _userRepositoryMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
            .Callback<User>(u => _updatedUser = u)
            .Returns(Task.CompletedTask);

        _hasherMock.Setup(h => h.Hash(It.IsAny<string>()))
            .Returns(_hashedPasswordResponse);

        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns(_generateTokenResponse);

        return new AccountService(
            _userRepositoryMock.Object,
            _hasherMock.Object,
            _jwtMock.Object,
            _userContextMock.Object);
    }

    // UpdateAccountCredentialsAsync

    [Fact]
    public async Task UpdateAccountCredentialsAsync_WhenUsernameIsAvailable_UpdatesCredentialsAndReturnsToken()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.MustChangePassword = true;
        user.AccessFailedCount = 3;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new UpdateAccountCredentialsDto("new-username", "new-password123");
        var service = GetService();

        // Act
        var result = await service.UpdateAccountCredentialsAsync(dto);

        // Assert
        AssertEx.AreEqual(TOKEN, result);
        AssertEx.AreEqual(dto.Username, _updatedUser!.Username);
        AssertEx.AreEqual(HASHED_PASSWORD, _updatedUser.PasswordHash);
        Assert.False(_updatedUser.MustChangePassword);
        AssertEx.AreEqual(0, _updatedUser.AccessFailedCount);
        Assert.Null(_updatedUser.LockoutEnd);

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(dto.Username), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.Is<User>(u =>
            u.Id == USER_ID &&
            u.Username == dto.Username &&
            u.PasswordHash == HASHED_PASSWORD &&
            !u.MustChangePassword &&
            u.AccessFailedCount == 0 &&
            u.LockoutEnd == null
        )), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Verify(dto.Password, originalPasswordHash), Times.Once);
        _hasherMock.Verify(h => h.Hash(dto.Password), Times.Once);
        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(user), Times.Once);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAccountCredentialsAsync_WhenUsernameIsUnchanged_UpdatesPasswordWithoutUsernameCheck()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.MustChangePassword = true;
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new UpdateAccountCredentialsDto(user.Username, "new-password123");
        var service = GetService();

        // Act
        var result = await service.UpdateAccountCredentialsAsync(dto);

        // Assert
        AssertEx.AreEqual(TOKEN, result);
        Assert.False(_updatedUser!.MustChangePassword);

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.Is<User>(u =>
            u.Id == USER_ID &&
            u.Username == user.Username &&
            u.PasswordHash == HASHED_PASSWORD &&
            !u.MustChangePassword
        )), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Verify(dto.Password, originalPasswordHash), Times.Once);
        _hasherMock.Verify(h => h.Hash(dto.Password), Times.Once);
        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(user), Times.Once);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAccountCredentialsAsync_WhenUsernameIsTaken_ThrowsUserAlreadyExistsException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.MustChangePassword = true;
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var existingUser = AuthTestData.DefaultUser;
        existingUser.Id = OTHER_USER_ID;
        existingUser.Username = "taken-username";
        _getUserByUsernameResponse = existingUser;

        var dto = new UpdateAccountCredentialsDto("taken-username", "new-password123");
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateAccountCredentialsAsync(dto);

        // Assert
        await AssertEx.Throws<UserAlreadyExistsException>(act, "already exists");
        Assert.True(user.MustChangePassword);

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(dto.Username), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Verify(dto.Password, originalPasswordHash), Times.Once);
        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAccountCredentialsAsync_WhenPasswordIsUnchanged_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.MustChangePassword = true;
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new UpdateAccountCredentialsDto("new-username", "temporary-password");
        var service = GetService();
        _hasherMock.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);

        // Act
        Func<Task> act = async () => await service.UpdateAccountCredentialsAsync(dto);

        // Assert
        await AssertEx.Throws<InvalidOperationException>(act, "must differ");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Verify(dto.Password, originalPasswordHash), Times.Once);
        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAccountCredentialsAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getUserByIdResponse = null;
        var dto = new UpdateAccountCredentialsDto("new-username", "new-password123");
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateAccountCredentialsAsync(dto);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"User with ID {USER_ID} not found");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAccountCredentialsAsync_WhenUserIsInactive_ThrowsInvalidCredentialException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.MustChangePassword = true;
        user.IsActive = false;
        _getUserByIdResponse = user;

        var dto = new UpdateAccountCredentialsDto("new-username", "new-password123");
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateAccountCredentialsAsync(dto);

        // Assert
        await AssertEx.Throws<InvalidCredentialException>(act, "Invalid credentials");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAccountCredentialsAsync_WhenPasswordChangeIsNotRequired_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.MustChangePassword = false;
        _getUserByIdResponse = user;

        var dto = new UpdateAccountCredentialsDto("new-username", "new-password123");
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateAccountCredentialsAsync(dto);

        // Assert
        await AssertEx.Throws<InvalidOperationException>(act, "password change is required");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }
}
