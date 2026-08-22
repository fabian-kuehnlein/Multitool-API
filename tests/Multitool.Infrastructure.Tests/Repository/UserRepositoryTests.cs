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
}
