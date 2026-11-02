using Mapster;
using Moq;
using Multitool.Application.Mappings;
using Multitool.Application.Models.User;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly Mock<IUserContext> _userContextMock;

    private List<User> _getUsersResponse;
    private User? _getUserByUsernameResponse;
    private User? _getUserByIdResponse;
    private int _createUserResponse;
    private string _hashedPasswordResponse;

    private static readonly int USER_ID = AuthTestData.DefaultUser.Id;
    private static readonly int OTHER_USER_ID = AuthTestData.DefaultUser.Id + 1;
    private const string HASHED_PASSWORD = "hashed-new-password";

    private User? _createdUser;
    private User? _updatedUser;

    public UserServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());

        _userRepositoryMock = new Mock<IUserRepository>();
        _hasherMock = new Mock<IPasswordHasher>();
        _userContextMock = new Mock<IUserContext>();
        _userContextMock.SetupGet(c => c.UserId).Returns(USER_ID);

        _getUsersResponse = [];
        _getUserByUsernameResponse = null;
        _getUserByIdResponse = AuthTestData.DefaultUser;
        _createUserResponse = AuthTestData.DefaultUser.Id;
        _hashedPasswordResponse = HASHED_PASSWORD;
    }

    private UserService GetService()
    {
        _createdUser = null;
        _updatedUser = null;

        _userRepositoryMock.Reset();
        _hasherMock.Reset();

        _userRepositoryMock.Setup(r => r.GetUsersAsync())
            .ReturnsAsync(_getUsersResponse);

        _userRepositoryMock.Setup(r => r.GetUserByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(_getUserByUsernameResponse);

        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(_getUserByIdResponse);

        _userRepositoryMock.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
            .Callback<User>(u => _createdUser = u)
            .ReturnsAsync(_createUserResponse);

        _userRepositoryMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
            .Callback<User>(u => _updatedUser = u)
            .Returns(Task.CompletedTask);

        _hasherMock.Setup(h => h.Hash(It.IsAny<string>()))
            .Returns(_hashedPasswordResponse);

        return new UserService(_userRepositoryMock.Object, _hasherMock.Object, _userContextMock.Object);
    }

    // GetUsersAsync

    [Fact]
    public async Task GetUsersAsync_WhenUsersExist_ReturnsMappedUserDtos()
    {
        // Arrange
        _getUsersResponse = [AuthTestData.DefaultUser];
        var service = GetService();

        // Act
        var result = await service.GetUsersAsync();

        // Assert
        AssertEx.AreEqual(result, new List<UserDto> { AuthTestData.DefaultUserDto });

        _userRepositoryMock.Verify(r => r.GetUsersAsync(), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    // CreateUserAsync

    [Fact]
    public async Task CreateUserAsync_WhenUsernameIsAvailable_CreatesActiveUserWithHashedPassword()
    {
        // Arrange
        var dto = AuthTestData.DefaultCreateUserDto;
        var service = GetService();

        // Act
        var result = await service.CreateUserAsync(dto);

        // Assert
        AssertEx.AreEqual(result, _createUserResponse);
        AssertEx.AreEqual(_createdUser, new User
        {
            Id = 0,
            Username = dto.Username,
            PasswordHash = HASHED_PASSWORD,
            Role = dto.Role,
            IsActive = true,
            CreatedAt = _createdUser!.CreatedAt
        });
        AssertEx.CloseTo(_createdUser!.CreatedAt, DateTime.Now, TimeSpan.FromSeconds(5));

        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(dto.Username), Times.Once);
        _userRepositoryMock.Verify(r => r.CreateUserAsync(It.Is<User>(u =>
            u.Id == 0 &&
            u.Username == dto.Username &&
            u.PasswordHash == HASHED_PASSWORD &&
            u.Role == dto.Role &&
            u.IsActive
        )), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Hash(dto.Password), Times.Once);
        _hasherMock.VerifyNoOtherCalls();

        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateUserAsync_WhenUsernameAlreadyExists_ThrowsUserAlreadyExistsException()
    {
        // Arrange
        _getUserByUsernameResponse = AuthTestData.DefaultUser;
        var dto = AuthTestData.DefaultCreateUserDto;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.CreateUserAsync(dto);

        // Assert
        await AssertEx.Throws<UserAlreadyExistsException>(act, "already exists");

        _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(dto.Username), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    // ResetPasswordAsync

    [Fact]
    public async Task ResetPasswordAsync_WhenUserExists_ResetsPasswordAndLockout()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.PasswordHash = "old-hash";
        user.AccessFailedCount = 3;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
        _getUserByIdResponse = user;
        var dto = AuthTestData.DefaultResetPasswordDto;
        var service = GetService();

        // Act
        await service.ResetPasswordAsync(USER_ID, dto);

        // Assert
        AssertEx.AreEqual(HASHED_PASSWORD, _updatedUser!.PasswordHash);
        AssertEx.AreEqual(0, _updatedUser.AccessFailedCount);
        Assert.Null(_updatedUser.LockoutEnd);

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.Is<User>(u =>
            u.Id == USER_ID &&
            u.PasswordHash == HASHED_PASSWORD &&
            u.AccessFailedCount == 0 &&
            u.LockoutEnd == null
        )), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.Verify(h => h.Hash(dto.Password), Times.Once);
        _hasherMock.VerifyNoOtherCalls();

        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getUserByIdResponse = null;
        var dto = AuthTestData.DefaultResetPasswordDto;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.ResetPasswordAsync(USER_ID, dto);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"User with ID {USER_ID} not found");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.VerifyNoOtherCalls();
        _userContextMock.VerifyNoOtherCalls();
    }

    // SetUserActiveAsync

    [Fact]
    public async Task SetUserActiveAsync_WhenDeactivatingAnotherUser_DeactivatesAndClearsLockout()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.Id = OTHER_USER_ID;
        user.IsActive = true;
        user.AccessFailedCount = 3;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
        _getUserByIdResponse = user;
        var dto = AuthTestData.DefaultSetUserActiveDto;
        var service = GetService();

        // Act
        await service.SetUserActiveAsync(OTHER_USER_ID, dto);

        // Assert
        Assert.False(_updatedUser!.IsActive);
        AssertEx.AreEqual(0, _updatedUser.AccessFailedCount);
        Assert.Null(_updatedUser.LockoutEnd);

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(OTHER_USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.Is<User>(u =>
            u.Id == OTHER_USER_ID &&
            !u.IsActive &&
            u.AccessFailedCount == 0 &&
            u.LockoutEnd == null
        )), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SetUserActiveAsync_WhenActivatingUser_ActivatesWithoutClearingLockout()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.Id = OTHER_USER_ID;
        user.IsActive = false;
        user.AccessFailedCount = 2;
        var lockout = DateTime.UtcNow.AddMinutes(10);
        user.LockoutEnd = lockout;
        _getUserByIdResponse = user;
        var dto = AuthTestData.DefaultSetUserActiveDto with { IsActive = true };
        var service = GetService();

        // Act
        await service.SetUserActiveAsync(OTHER_USER_ID, dto);

        // Assert
        Assert.True(_updatedUser!.IsActive);
        AssertEx.AreEqual(2, _updatedUser.AccessFailedCount);
        AssertEx.AreEqual(lockout, _updatedUser.LockoutEnd);

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(OTHER_USER_ID), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.Is<User>(u =>
            u.Id == OTHER_USER_ID &&
            u.IsActive &&
            u.AccessFailedCount == 2
        )), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Never);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SetUserActiveAsync_WhenDeactivatingOwnAccount_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.IsActive = true;
        _getUserByIdResponse = user;
        var dto = AuthTestData.DefaultSetUserActiveDto;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.SetUserActiveAsync(USER_ID, dto);

        // Assert
        await AssertEx.Throws<InvalidOperationException>(act, "cannot deactivate your own account");
        Assert.True(user.IsActive);

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(USER_ID), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Once);
        _userContextMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SetUserActiveAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getUserByIdResponse = null;
        var dto = AuthTestData.DefaultSetUserActiveDto;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.SetUserActiveAsync(OTHER_USER_ID, dto);

        // Assert
        await AssertEx.Throws<NotFoundException>(act, $"User with ID {OTHER_USER_ID} not found");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(OTHER_USER_ID), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();

        _hasherMock.VerifyNoOtherCalls();

        _userContextMock.VerifyGet(c => c.UserId, Times.Never);
        _userContextMock.VerifyNoOtherCalls();
    }
}
