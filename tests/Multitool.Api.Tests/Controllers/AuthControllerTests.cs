using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Tests.Shared;

namespace Multitool.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthenticationService> _serviceMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _serviceMock = new Mock<IAuthenticationService>();
        _sut = new AuthController(_serviceMock.Object);
    }

    // POST api/Auth/register

    [Fact]
    public async Task Register_WhenRequestIsValid_ReturnsOk()
    {
        var request = AuthTestData.DefaultRegisterRequest;
        const string adminKey = AuthTestData.ValidAdminKey;

        var result = await _sut.Register(request, adminKey);

        result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.RegisterAsync(request.Username, request.Password, adminKey), Times.Once);
    }

    // POST api/Auth/login

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ReturnsOkWithToken()
    {
        var request = AuthTestData.DefaultLoginRequest;
        _serviceMock.Setup(s => s.LoginAsync(request.Username, request.Password)).ReturnsAsync("token-123");

        var result = await _sut.Login(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new { token = "token-123" });
    }
}
