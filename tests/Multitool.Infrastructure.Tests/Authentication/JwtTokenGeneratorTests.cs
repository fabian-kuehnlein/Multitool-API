using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Exceptions;
using Multitool.Infrastructure.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Multitool.Infrastructure.Tests;

public class JwtTokenGeneratorTests
{
    private static IConfiguration BuildConfig(Dictionary<string, string?> settings)
        => new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

    private static readonly Dictionary<string, string?> ValidSettings = new()
    {
        {"Jwt:Key", "super-secret-key-that-is-at-least-32-characters-long"},
        {"Jwt:Issuer", "test-issuer"},
        {"Jwt:Audience", "test-audience"}
    };

    // GenerateToken

    [Fact]
    public void GenerateToken_WhenConfigIsValid_ReturnsTokenWithCorrectIssuerAndAudience()
    {
        // Arrange
        var sut = new JwtTokenGenerator(BuildConfig(ValidSettings));
        var user = new User { Id = 1, Username = "testuser" };

        // Act
        var token = sut.GenerateToken(user);

        // Assert
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwtToken.Issuer.Should().Be("test-issuer");
        jwtToken.Audiences.Should().Contain("test-audience");
    }

    [Fact]
    public void GenerateToken_WhenConfigIsValid_IncludesUserIdAndUsernameClaims()
    {
        // Arrange
        var sut = new JwtTokenGenerator(BuildConfig(ValidSettings));
        var user = new User { Id = 42, Username = "testuser" };

        // Act
        var token = sut.GenerateToken(user);

        // Assert
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "42");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "testuser");
    }

    [Fact]
    public void GenerateToken_WhenConfigIsValid_SetsExpiryToEndOfDay()
    {
        // Arrange
        var sut = new JwtTokenGenerator(BuildConfig(ValidSettings));
        var user = new User { Id = 1, Username = "testuser" };
        var before = DateTime.UtcNow;

        // Act
        var token = sut.GenerateToken(user);

        // Assert
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwtToken.ValidTo.Should().BeCloseTo(before.AddDays(1), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateToken_WhenJwtKeyIsMissing_ThrowsJwtMissingException()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            {"Jwt:Issuer", "test-issuer"},
            {"Jwt:Audience", "test-audience"}
        };
        var sut = new JwtTokenGenerator(BuildConfig(settings));
        var user = new User { Id = 1, Username = "testuser" };

        // Act
        var act = () => sut.GenerateToken(user);

        // Assert
        act.Should().Throw<JwtMissingException>();
    }
}
