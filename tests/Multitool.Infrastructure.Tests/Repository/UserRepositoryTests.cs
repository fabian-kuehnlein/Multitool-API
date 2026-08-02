using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Config;
using Multitool.Infrastructure.Repositories;

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
        var user = new User { Username = "test", PasswordHash = "hash" };

        // Act
        await _sut.AddAsync(user);

        // Assert
        var dbUser = await Context.Users.FirstOrDefaultAsync(u => u.Username == "test");
        dbUser.Should().NotBeNull();
        dbUser!.PasswordHash.Should().Be("hash");
    }

    // GetByUsernameAsync

    [Fact]
    public async Task GetByUsernameAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = new User { Username = "findme", PasswordHash = "hash" };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByUsernameAsync("findme");

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("findme");
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByUsernameAsync("nobody");

        // Assert
        result.Should().BeNull();
    }
}
