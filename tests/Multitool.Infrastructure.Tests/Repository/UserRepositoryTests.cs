using Microsoft.EntityFrameworkCore;
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

    // AddAsync

    [Fact]
    public async Task AddAsync_WhenUserIsValid_AddsUserToDatabase()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.Username = "test";

        // Act
        await _sut.AddAsync(user);

        // Assert
        var dbUser = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == "test");
        Assert.NotNull(dbUser);
        AssertEx.AreEqual(user.PasswordHash, dbUser!.PasswordHash);
    }

    // GetByUsernameAsync

    [Fact]
    public async Task GetByUsernameAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.Username = "findme";
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByUsernameAsync("findme");

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual("findme", result!.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _sut.GetByUsernameAsync("nobody");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenMultipleUsersExist_ReturnsCorrectUser()
    {
        // Arrange
        var user1 = AuthTestData.DefaultUser;
        user1.Id = 0;
        user1.Username = "alice";
        var user2 = new Multitool.Domain.Entities.Config.User { Id = 0, Username = "bob", PasswordHash = "hash" };
        Context.Users.AddRange(user1, user2);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByUsernameAsync("bob");

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual("bob", result!.Username);
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenUserExists_UpdatesUsername()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.Username = "oldname";
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        user.Username = "newname";

        // Act
        await _sut.UpdateAsync(user);

        // Assert
        var dbUser = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(dbUser);
        AssertEx.AreEqual("newname", dbUser!.Username);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_UpdatesPasswordHash()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.PasswordHash = "old-hash";
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        user.PasswordHash = "new-hash";

        // Act
        await _sut.UpdateAsync(user);

        // Assert
        var dbUser = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(dbUser);
        AssertEx.AreEqual("new-hash", dbUser!.PasswordHash);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_UpdatesLockoutEnd()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.LockoutEnd = null;
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var lockout = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        user.LockoutEnd = lockout;

        // Act
        await _sut.UpdateAsync(user);

        // Assert
        var dbUser = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(dbUser);
        AssertEx.AreEqual(lockout, dbUser!.LockoutEnd);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_IncrementsAccessFailedCount()
    {
        // Arrange
        var user = AuthTestData.DefaultUser;
        user.AccessFailedCount = 0;
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        user.AccessFailedCount = 3;

        // Act
        await _sut.UpdateAsync(user);

        // Assert
        var dbUser = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(dbUser);
        AssertEx.AreEqual(3, dbUser!.AccessFailedCount);
    }
}
