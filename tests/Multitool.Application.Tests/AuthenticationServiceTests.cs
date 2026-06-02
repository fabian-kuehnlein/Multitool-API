using System.Security.Authentication;
using FluentAssertions;
using Moq;
using Multitool.Api.Extensions;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;

namespace Multitool.Application.Tests;

public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtMock;
    private readonly Mock<IAdminKeyProvider> _keyProviderMock;
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _hasherMock = new Mock<IPasswordHasher>();
        _jwtMock = new Mock<IJwtTokenGenerator>();
        _keyProviderMock = new Mock<IAdminKeyProvider>();
        _sut = new AuthenticationService(_userRepositoryMock.Object, _hasherMock.Object, _jwtMock.Object, _keyProviderMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidAdminKey_AndNewUser_AddsUser()
    {
        var user = AuthTestData.DefaultUser;
        var request = AuthTestData.DefaultRegisterRequest;
        const string adminKey = AuthTestData.ValidAdminKey;

        _keyProviderMock.Setup(p => p.GetAdminKey()).Returns(adminKey);
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync(request.Username)).ReturnsAsync((User?)null);
        _hasherMock.Setup(h => h.Hash(request.Password)).Returns(user.PasswordHash);

        await _sut.RegisterAsync(request.Username, request.Password, adminKey);

        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Username == request.Username && u.PasswordHash == user.PasswordHash)), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidAdminKey_ThrowsInvalidCredentialException()
    {
        _keyProviderMock.Setup(p => p.GetAdminKey()).Returns(AuthTestData.ValidAdminKey);

        var act = () => _sut.RegisterAsync("user", "pass", "wrong-key");

        await act.Should().ThrowAsync<InvalidCredentialException>();
    }

    [Fact]
    public async Task RegisterAsync_WhenUserAlreadyExists_ThrowsUserAlreadyExistsException()
    {
        const string adminKey = AuthTestData.ValidAdminKey;
        _keyProviderMock.Setup(p => p.GetAdminKey()).Returns(adminKey);
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("existing")).ReturnsAsync(new User());

        var act = () => _sut.RegisterAsync("existing", "pass", adminKey);

        await act.Should().ThrowAsync<UserAlreadyExistsException>();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken_AndResetsLockout()
    {
        var user = AuthTestData.DefaultUser;
        user.AccessFailedCount = 3;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(-5); // In the past
        var request = AuthTestData.DefaultLoginRequest;

        _userRepositoryMock.Setup(r => r.GetByUsernameAsync(request.Username)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(request.Password, user.PasswordHash)).Returns(true);
        _jwtMock.Setup(j => j.GenerateToken(user)).Returns("token-123");

        var result = await _sut.LoginAsync(request.Username, request.Password);

        result.Should().Be("token-123");
        user.AccessFailedCount.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenLockedOut_ThrowsInvalidCredentialException()
    {
        var user = AuthTestData.DefaultUser;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
        var request = AuthTestData.DefaultLoginRequest;

        _userRepositoryMock.Setup(r => r.GetByUsernameAsync(request.Username)).ReturnsAsync(user);

        var act = () => _sut.LoginAsync(request.Username, request.Password);

        await act.Should().ThrowAsync<InvalidCredentialException>();
        _hasherMock.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_IncrementsFailureCount()
    {
        var user = AuthTestData.DefaultUser;
        user.AccessFailedCount = 0;
        var request = AuthTestData.DefaultLoginRequest;

        _userRepositoryMock.Setup(r => r.GetByUsernameAsync(request.Username)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(request.Password, user.PasswordHash)).Returns(false);

        var act = () => _sut.LoginAsync(request.Username, request.Password);

        await act.Should().ThrowAsync<InvalidCredentialException>();
        user.AccessFailedCount.Should().Be(1);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_After5Failures_SetsLockoutEnd()
    {
        var user = new User 
        { 
            Id = 1, 
            Username = "lockout-user", 
            PasswordHash = "hash",
            AccessFailedCount = 4 
        };
        var request = new LoginRequest(user.Username, "wrong-pass");

        _userRepositoryMock.Setup(r => r.GetByUsernameAsync(user.Username)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(request.Password, user.PasswordHash)).Returns(false);

        var act = () => _sut.LoginAsync(user.Username, request.Password);

        await act.Should().ThrowAsync<InvalidCredentialException>();
        
        user.AccessFailedCount.Should().Be(5);
        user.LockoutEnd.Should().NotBeNull();
        user.LockoutEnd.Value.Should().BeAfter(DateTime.UtcNow);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.AccessFailedCount == 5)), Times.Once);
    }
}
