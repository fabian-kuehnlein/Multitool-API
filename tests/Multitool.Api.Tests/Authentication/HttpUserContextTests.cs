using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Multitool.Api.Authentication;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Api.Tests.Authentication;

public class HttpUserContextTests
{
    private static HttpUserContext CreateSut(ClaimsPrincipal? principal)
    {
        var accessor = new HttpContextAccessor();

        if (principal != null)
            accessor.HttpContext = new DefaultHttpContext { User = principal };

        return new HttpUserContext(accessor);
    }

    private static ClaimsPrincipal CreatePrincipal(string? userId)
    {
        var claims = userId == null
            ? Array.Empty<Claim>()
            : new[] { new Claim(ClaimTypes.NameIdentifier, userId) };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    // UserId

    [Fact]
    public void UserId_WhenRequestHasUserIdClaim_ReturnsUserId()
    {
        // Arrange
        var sut = CreateSut(CreatePrincipal("42"));

        // Act
        var result = sut.UserId;

        // Assert
        AssertEx.AreEqual(42, result);
    }

    [Fact]
    public void UserId_WhenHttpContextIsNull_ReturnsZero()
    {
        // Arrange
        var sut = CreateSut(principal: null);

        // Act
        var result = sut.UserId;

        // Assert
        AssertEx.AreEqual(0, result);
    }

    [Fact]
    public void UserId_WhenUserIdClaimIsMissing_ReturnsZero()
    {
        // Arrange
        var sut = CreateSut(CreatePrincipal(userId: null));

        // Act
        var result = sut.UserId;

        // Assert
        AssertEx.AreEqual(0, result);
    }

    [Theory]
    [InlineData("not-a-number")]
    [InlineData("")]
    public void UserId_WhenUserIdClaimIsInvalid_ReturnsZero(string userId)
    {
        // Arrange
        var sut = CreateSut(CreatePrincipal(userId));

        // Act
        var result = sut.UserId;

        // Assert
        AssertEx.AreEqual(0, result);
    }
}
