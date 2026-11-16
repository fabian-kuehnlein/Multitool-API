using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Account;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Api.Tests.Controllers;

public class AccountControllerTests
{
    private readonly Mock<IAccountService> _accountServiceMock;

    private string _credentialsResponse;

    private const string Token = "token-456";

    public AccountControllerTests()
    {
        _accountServiceMock = new Mock<IAccountService>();

        // Default responses
        _credentialsResponse = Token;
    }

    private AccountController GetController()
    {
        _accountServiceMock.Reset();

        _accountServiceMock
            .Setup(s => s.VerifyAccountCredentialsAsync(It.IsAny<VerifyAccountCredentialsRequest>()))
            .Returns(Task.CompletedTask);

        _accountServiceMock
            .Setup(s => s.SetupAccountCredentialsAsync(It.IsAny<SetupAccountCredentialsRequest>()))
            .ReturnsAsync(_credentialsResponse);

        _accountServiceMock
            .Setup(s => s.ChangeAccountCredentialsAsync(It.IsAny<ChangeAccountCredentialsRequest>()))
            .ReturnsAsync(_credentialsResponse);

        return new AccountController(_accountServiceMock.Object);
    }

    // POST api/Account/credentials/verify

    [Fact]
    public async Task VerifyCredentials_WhenCredentialsAreValid_ReturnsNoContent()
    {
        // Arrange
        var dto = AuthTestData.DefaultVerifyAccountCredentialsRequest;
        var controller = GetController();

        // Act
        var result = await controller.VerifyCredentials(dto);

        // Assert
        AssertEx.NoContent(result);

        _accountServiceMock.Verify(s => s.VerifyAccountCredentialsAsync(dto), Times.Once);
        _accountServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/Account/credentials/setup

    [Fact]
    public async Task SetupCredentials_WhenCredentialsAreValid_ReturnsOkWithToken()
    {
        // Arrange
        var dto = AuthTestData.DefaultSetupAccountCredentialsRequest;
        var controller = GetController();

        // Act
        var result = await controller.SetupCredentials(dto);

        // Assert
        AssertEx.Ok(result, new { token = _credentialsResponse });

        _accountServiceMock.Verify(s => s.SetupAccountCredentialsAsync(dto), Times.Once);
        _accountServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/Account/credentials

    [Fact]
    public async Task ChangeCredentials_WhenCredentialsAreValid_ReturnsOkWithToken()
    {
        // Arrange
        var dto = AuthTestData.DefaultChangeAccountCredentialsRequest;
        var controller = GetController();

        // Act
        var result = await controller.ChangeCredentials(dto);

        // Assert
        AssertEx.Ok(result, new { token = _credentialsResponse });

        _accountServiceMock.Verify(s => s.ChangeAccountCredentialsAsync(dto), Times.Once);
        _accountServiceMock.VerifyNoOtherCalls();
    }
}
