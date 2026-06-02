using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Multitool.Domain.Entities.Config;
using Multitool.Infrastructure.Authentification;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Multitool.Infrastructure.Tests;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_ReturnsValidJwt()
    {
        var inMemorySettings = new Dictionary<string, string?> {
            {"Jwt:Key", "super-secret-key-that-is-at-least-32-characters-long"},
            {"Jwt:Issuer", "test-issuer"},
            {"Jwt:Audience", "test-audience"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var sut = new JwtTokenGenerator(configuration);
        var user = new User { Id = 1, Username = "testuser" };

        var token = sut.GenerateToken(user);

        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("test-issuer");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "testuser");
    }
}
