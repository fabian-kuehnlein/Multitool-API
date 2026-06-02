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

    [Fact]
    public async Task AddAsync_AddsUser()
    {
        var user = new User { Username = "test", PasswordHash = "hash" };
        
        await _sut.AddAsync(user);

        var dbUser = await Context.Users.FirstOrDefaultAsync(u => u.Username == "test");
        dbUser.Should().NotBeNull();
        dbUser!.PasswordHash.Should().Be("hash");
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsCorrectUser()
    {
        var user = new User { Username = "findme", PasswordHash = "hash" };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var result = await _sut.GetByUsernameAsync("findme");

        result.Should().NotBeNull();
        result!.Username.Should().Be("findme");
    }
}
