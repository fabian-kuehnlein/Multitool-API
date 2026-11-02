using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Enums;
using Multitool.Infrastructure.Repositories;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Infrastructure.Tests;

public class UserRepositoryTests : RepositoryTestBase
{
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        _sut = new UserRepository(Context);
    }

    // CreateUserAsync

    [Fact]
    public async Task CreateUserAsync_WhenUserIsValid_AddsUserToDatabaseAndReturnsId()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.Id = 0;
        user.Username = "test";

        // Act
        var result = await _sut.CreateUserAsync(user);

        // Assert
        AssertEx.AreEqual(user.Id, result);

        var dbUser = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == "test");
        Assert.NotNull(dbUser);
        AssertEx.AreEqual(user.Username, dbUser!.Username);
        AssertEx.AreEqual(user.PasswordHash, dbUser.PasswordHash);
        AssertEx.AreEqual(user.Role, dbUser.Role);
        AssertEx.AreEqual(user.IsActive, dbUser.IsActive);
        AssertEx.AreEqual(user.CreatedAt, dbUser.CreatedAt);
    }

    // GetUserByIdAsync

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetUserByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(user.Id, result!.Id);
        AssertEx.AreEqual(user.Username, result.Username);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _sut.GetUserByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    // GetUsersAsync

    [Fact]
    public async Task GetUsersAsync_WhenMultipleUsersExist_ReturnsUsersSortedByUsername()
    {
        // Arrange
        var charlie = AuthTestData.DefaultUser;
        charlie.Id = 0;
        charlie.Username = "charlie";
        var alice = AuthTestData.DefaultUser;
        alice.Id = 0;
        alice.Username = "alice";
        var bob = AuthTestData.DefaultUser;
        bob.Id = 0;
        bob.Username = "bob";
        Context.Users.AddRange(charlie, alice, bob);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetUsersAsync();

        // Assert
        AssertEx.AreEqual(result.Select(u => u.Username).ToList(), new List<string> { "alice", "bob", "charlie" });
    }

    [Fact]
    public async Task GetUsersAsync_WhenNoUsersExist_ReturnsEmptyList()
    {
        // Arrange

        // Act
        var result = await _sut.GetUsersAsync();

        // Assert
        Assert.Empty(result);
    }

    // GetUserByUsernameAsync

    [Fact]
    public async Task GetUserByUsernameAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.Username = "findme";
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetUserByUsernameAsync("findme");

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual("findme", result!.Username);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _sut.GetUserByUsernameAsync("nobody");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_WhenMultipleUsersExist_ReturnsCorrectUser()
    {
        // Arrange
        var user1 = AuthTestData.DefaultUser;
        user1.Id = 0;
        user1.Username = "alice";
        var user2 = new Multitool.Domain.Entities.Config.User { Id = 0, Username = "bob", PasswordHash = "hash" };
        Context.Users.AddRange(user1, user2);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetUserByUsernameAsync("bob");

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual("bob", result!.Username);
    }

    // UpdateUserAsync

    [Fact]
    public async Task UpdateUserAsync_WhenUserExists_UpdatesUsername()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.Username = "oldname";
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        user.Username = "newname";

        // Act
        await _sut.UpdateUserAsync(user);

        // Assert
        var dbUser = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(dbUser);
        AssertEx.AreEqual("newname", dbUser!.Username);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserExists_UpdatesPasswordHash()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.PasswordHash = "old-hash";
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        user.PasswordHash = "new-hash";

        // Act
        await _sut.UpdateUserAsync(user);

        // Assert
        var dbUser = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(dbUser);
        AssertEx.AreEqual("new-hash", dbUser!.PasswordHash);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserExists_UpdatesLockoutEnd()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.LockoutEnd = null;
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var lockout = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        user.LockoutEnd = lockout;

        // Act
        await _sut.UpdateUserAsync(user);

        // Assert
        var dbUser = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(dbUser);
        AssertEx.AreEqual(lockout, dbUser!.LockoutEnd);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserExists_IncrementsAccessFailedCount()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.AccessFailedCount = 0;
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        user.AccessFailedCount = 3;

        // Act
        await _sut.UpdateUserAsync(user);

        // Assert
        var dbUser = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(dbUser);
        AssertEx.AreEqual(3, dbUser!.AccessFailedCount);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserExists_UpdatesRoleAndIsActive()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.Role = UserRole.Admin;
        user.IsActive = true;
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        user.Role = UserRole.User;
        user.IsActive = false;

        // Act
        await _sut.UpdateUserAsync(user);

        // Assert
        var dbUser = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(dbUser);
        AssertEx.AreEqual(UserRole.User, dbUser!.Role);
        Assert.False(dbUser.IsActive);
    }
}
