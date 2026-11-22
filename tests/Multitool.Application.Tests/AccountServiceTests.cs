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
    private const string CURRENT_PASSWORD = "current-password";
    private const string NEW_PASSWORD = "new-password123";

    private User? _updatedUser;

    public AccountServiceTests()
    {
        MappingConfigBootstrapper.EnsureApplied();

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

        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

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

    // VerifyAccountCredentialsAsync

    [Fact]
    public async Task VerifyAccountCredentialsAsync_WhenCredentialsAreCorrect_CompletesWithoutChanges()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new VerifyAccountCredentialsRequest(user.Username, CURRENT_PASSWORD);
        var service = GetService();
        _hasherMock.Setup(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash)).Returns(true);

        // Act
        await service.VerifyAccountCredentialsAsync(dto);

        // Assert
        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash), Times.Once);
        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task VerifyAccountCredentialsAsync_WhenUsernameIsWrong_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        _getUserByIdResponse = user;

        var dto = new VerifyAccountCredentialsRequest("wrong-username", CURRENT_PASSWORD);
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.VerifyAccountCredentialsAsync(dto);

        // Assert
        await AssertEx.Throws<InvalidOperationException>(act, "current credentials are incorrect");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task VerifyAccountCredentialsAsync_WhenPasswordIsWrong_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new VerifyAccountCredentialsRequest(user.Username, "wrong-password");
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.VerifyAccountCredentialsAsync(dto);

        // Assert
        await AssertEx.Throws<InvalidOperationException>(act, "current credentials are incorrect");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
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
    public async Task VerifyAccountCredentialsAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getUserByIdResponse = null;
        var dto = AuthTestData.DefaultVerifyAccountCredentialsRequest;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.VerifyAccountCredentialsAsync(dto);

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
    public async Task VerifyAccountCredentialsAsync_WhenUserIsInactive_ThrowsInvalidCredentialException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.IsActive = false;
        _getUserByIdResponse = user;

        var dto = AuthTestData.DefaultVerifyAccountCredentialsRequest;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.VerifyAccountCredentialsAsync(dto);

        // Assert
        await AssertEx.Throws<InvalidCredentialException>(act, "Invalid credentials");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    // SetupAccountCredentialsAsync

    [Fact]
    public async Task SetupAccountCredentialsAsync_WhenUsernameIsAvailable_UpdatesCredentialsAndReturnsToken()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.MustChangePassword = true;
        user.AccessFailedCount = 3;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new SetupAccountCredentialsRequest("new-username", NEW_PASSWORD);
        var service = GetService();

        // Act
        var result = await service.SetupAccountCredentialsAsync(dto);

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
    public async Task SetupAccountCredentialsAsync_WhenUsernameIsUnchanged_UpdatesPasswordWithoutUsernameCheck()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.MustChangePassword = true;
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new SetupAccountCredentialsRequest(user.Username, NEW_PASSWORD);
        var service = GetService();

        // Act
        var result = await service.SetupAccountCredentialsAsync(dto);

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
    public async Task SetupAccountCredentialsAsync_WhenUsernameIsTaken_ThrowsUserAlreadyExistsException()
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

        var dto = new SetupAccountCredentialsRequest("taken-username", NEW_PASSWORD);
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.SetupAccountCredentialsAsync(dto);

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
    public async Task SetupAccountCredentialsAsync_WhenPasswordIsUnchanged_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.MustChangePassword = true;
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new SetupAccountCredentialsRequest("new-username", "temporary-password");
        var service = GetService();
        _hasherMock.Setup(h => h.Verify(dto.Password, originalPasswordHash)).Returns(true);

        // Act
        Func<Task> act = async () => await service.SetupAccountCredentialsAsync(dto);

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
    public async Task SetupAccountCredentialsAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getUserByIdResponse = null;
        var dto = new SetupAccountCredentialsRequest("new-username", NEW_PASSWORD);
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.SetupAccountCredentialsAsync(dto);

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
    public async Task SetupAccountCredentialsAsync_WhenUserIsInactive_ThrowsInvalidCredentialException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.MustChangePassword = true;
        user.IsActive = false;
        _getUserByIdResponse = user;

        var dto = new SetupAccountCredentialsRequest("new-username", NEW_PASSWORD);
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.SetupAccountCredentialsAsync(dto);

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
    public async Task SetupAccountCredentialsAsync_WhenPasswordChangeIsNotRequired_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.MustChangePassword = false;
        _getUserByIdResponse = user;

        var dto = new SetupAccountCredentialsRequest("new-username", NEW_PASSWORD);
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.SetupAccountCredentialsAsync(dto);

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

    // ChangeAccountCredentialsAsync

    [Fact]
    public async Task ChangeAccountCredentialsAsync_WhenUsernameAndPasswordAreValid_UpdatesBothAndReturnsToken()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.AccessFailedCount = 4;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new ChangeAccountCredentialsRequest(CURRENT_PASSWORD, "new-username", NEW_PASSWORD);
        var service = GetService();
        _hasherMock.Setup(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash)).Returns(true);

        // Act
        var result = await service.ChangeAccountCredentialsAsync(dto);

        // Assert
        AssertEx.AreEqual(TOKEN, result);
        AssertEx.AreEqual(dto.NewUsername, _updatedUser!.Username);
        AssertEx.AreEqual(HASHED_PASSWORD, _updatedUser.PasswordHash);
        AssertEx.AreEqual(0, _updatedUser.AccessFailedCount);
        Assert.Null(_updatedUser.LockoutEnd);

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(dto.NewUsername!), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.Is<User>(u =>
            u.Id == USER_ID &&
            u.Username == dto.NewUsername &&
            u.PasswordHash == HASHED_PASSWORD &&
            u.AccessFailedCount == 0 &&
            u.LockoutEnd == null
        )), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash), Times.Once);
        _hasherMock.Verify(h => h.Verify(NEW_PASSWORD, originalPasswordHash), Times.Once);
        _hasherMock.Verify(h => h.Hash(NEW_PASSWORD), Times.Once);
        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(user), Times.Once);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangeAccountCredentialsAsync_WhenOnlyUsernameChanges_UpdatesUsernameWithoutHashingPassword()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new ChangeAccountCredentialsRequest(CURRENT_PASSWORD, "new-username", null);
        var service = GetService();
        _hasherMock.Setup(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash)).Returns(true);

        // Act
        var result = await service.ChangeAccountCredentialsAsync(dto);

        // Assert
        AssertEx.AreEqual(TOKEN, result);
        AssertEx.AreEqual(dto.NewUsername, _updatedUser!.Username);
        AssertEx.AreEqual(originalPasswordHash, _updatedUser.PasswordHash);

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(dto.NewUsername!), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.Is<User>(u =>
            u.Id == USER_ID &&
            u.Username == dto.NewUsername &&
            u.PasswordHash == originalPasswordHash
        )), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash), Times.Once);
        _hasherMock.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(user), Times.Once);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangeAccountCredentialsAsync_WhenOnlyPasswordChanges_UpdatesPasswordWithoutUsernameCheck()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new ChangeAccountCredentialsRequest(CURRENT_PASSWORD, null, NEW_PASSWORD);
        var service = GetService();
        _hasherMock.Setup(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash)).Returns(true);

        // Act
        var result = await service.ChangeAccountCredentialsAsync(dto);

        // Assert
        AssertEx.AreEqual(TOKEN, result);
        AssertEx.AreEqual(user.Username, _updatedUser!.Username);
        AssertEx.AreEqual(HASHED_PASSWORD, _updatedUser.PasswordHash);

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.Is<User>(u =>
            u.Id == USER_ID &&
            u.Username == user.Username &&
            u.PasswordHash == HASHED_PASSWORD
        )), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash), Times.Once);
        _hasherMock.Verify(h => h.Verify(NEW_PASSWORD, originalPasswordHash), Times.Once);
        _hasherMock.Verify(h => h.Hash(NEW_PASSWORD), Times.Once);
        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(user), Times.Once);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangeAccountCredentialsAsync_WhenCurrentPasswordIsWrong_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new ChangeAccountCredentialsRequest("wrong-password", "new-username", NEW_PASSWORD);
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.ChangeAccountCredentialsAsync(dto);

        // Assert
        await AssertEx.Throws<InvalidOperationException>(act, "current password is incorrect");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Verify(dto.CurrentPassword, originalPasswordHash), Times.Once);
        _hasherMock.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangeAccountCredentialsAsync_WhenNoChangesAreProvided_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new ChangeAccountCredentialsRequest(CURRENT_PASSWORD, null, null);
        var service = GetService();
        _hasherMock.Setup(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash)).Returns(true);

        // Act
        Func<Task> act = async () => await service.ChangeAccountCredentialsAsync(dto);

        // Assert
        await AssertEx.Throws<InvalidOperationException>(act, "At least one new credential");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash), Times.Once);
        _hasherMock.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangeAccountCredentialsAsync_WhenUsernameIsTaken_ThrowsUserAlreadyExistsException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var existingUser = AuthTestData.DefaultUser;
        existingUser.Id = OTHER_USER_ID;
        existingUser.Username = "taken-username";
        _getUserByUsernameResponse = existingUser;

        var dto = new ChangeAccountCredentialsRequest(CURRENT_PASSWORD, "taken-username", null);
        var service = GetService();
        _hasherMock.Setup(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash)).Returns(true);

        // Act
        Func<Task> act = async () => await service.ChangeAccountCredentialsAsync(dto);

        // Assert
        await AssertEx.Throws<UserAlreadyExistsException>(act, "already exists");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(dto.NewUsername!), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash), Times.Once);
        _hasherMock.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangeAccountCredentialsAsync_WhenNewPasswordEqualsCurrentPassword_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        var originalPasswordHash = user.PasswordHash;
        _getUserByIdResponse = user;

        var dto = new ChangeAccountCredentialsRequest(CURRENT_PASSWORD, null, CURRENT_PASSWORD);
        var service = GetService();
        _hasherMock.Setup(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash)).Returns(true);

        // Act
        Func<Task> act = async () => await service.ChangeAccountCredentialsAsync(dto);

        // Assert
        await AssertEx.Throws<InvalidOperationException>(act, "must differ");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Verify(CURRENT_PASSWORD, originalPasswordHash), Times.Exactly(2));
        _hasherMock.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ChangeAccountCredentialsAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getUserByIdResponse = null;
        var dto = new ChangeAccountCredentialsRequest(CURRENT_PASSWORD, "new-username", NEW_PASSWORD);
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.ChangeAccountCredentialsAsync(dto);

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
    public async Task ChangeAccountCredentialsAsync_WhenUserIsInactive_ThrowsInvalidCredentialException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.IsActive = false;
        _getUserByIdResponse = user;

        var dto = new ChangeAccountCredentialsRequest(CURRENT_PASSWORD, "new-username", NEW_PASSWORD);
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.ChangeAccountCredentialsAsync(dto);

        // Assert
        await AssertEx.Throws<InvalidCredentialException>(act, "Invalid credentials");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.VerifyNoOtherCalls();

        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _jwtMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }
}
