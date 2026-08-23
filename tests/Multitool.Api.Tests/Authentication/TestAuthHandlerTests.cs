using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Multitool.Api.Authentication;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Api.Tests.Authentication;

public class TestAuthHandlerTests
{
    private const string SchemeName = TestAuthHandler.SchemeName;

    private TestAuthHandler CreateHandler()
    {
        var optionsMonitorMock = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        optionsMonitorMock.Setup(o => o.Get(SchemeName)).Returns(new AuthenticationSchemeOptions());

        var handler = new TestAuthHandler(
            optionsMonitorMock.Object,
            NullLoggerFactory.Instance,
            UrlEncoder.Default);

        var scheme = new AuthenticationScheme(SchemeName, SchemeName, typeof(TestAuthHandler));
        handler.InitializeAsync(scheme, new DefaultHttpContext()).GetAwaiter().GetResult();

        return handler;
    }

    // HandleAuthenticateAsync

    [Fact]
    public async Task HandleAuthenticateAsync_WhenCalled_ReturnsSuccessResult()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhenCalled_ReturnsAuthenticatedPrincipalWithTestScheme()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        var principal = result.Principal;
        Assert.NotNull(principal);
        Assert.True(principal.Identity?.IsAuthenticated);
        AssertEx.AreEqual(SchemeName, principal.Identity?.AuthenticationType);
    }
}
