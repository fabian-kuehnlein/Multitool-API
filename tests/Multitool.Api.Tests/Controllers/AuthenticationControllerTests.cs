using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthenticationService> _authenticationServiceMock;

    private string _loginResponse;

    private const string Token = "token-123";

    public AuthControllerTests()
    {
        _authenticationServiceMock = new Mock<IAuthenticationService>();

        // Default responses
        _loginResponse = Token;
    }

    private AuthController GetController()
    {
        _authenticationServiceMock.Reset();

        _authenticationServiceMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(_loginResponse);

        return new AuthController(_authenticationServiceMock.Object);
    }

    // POST api/Auth/login

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ReturnsOkWithToken()
    {
        // Arrange
        var request = AuthTestData.DefaultLoginRequest;
        var controller = GetController();

        // Act
        var result = await controller.Login(request);

        // Assert
        AssertEx.Ok(result, new { token = _loginResponse });

        _authenticationServiceMock.Verify(s => s.LoginAsync(request.Username, request.Password), Times.Once);
        _authenticationServiceMock.VerifyNoOtherCalls();
    }
}
