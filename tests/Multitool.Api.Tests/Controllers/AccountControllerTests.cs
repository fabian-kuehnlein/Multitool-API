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

    private string _updateCredentialsResponse;

    private const string Token = "token-456";

    public AccountControllerTests()
    {
        _accountServiceMock = new Mock<IAccountService>();

        // Default responses
        _updateCredentialsResponse = Token;
    }

    private AccountController GetController()
    {
        _accountServiceMock.Reset();

        _accountServiceMock
            .Setup(s => s.UpdateAccountCredentialsAsync(It.IsAny<UpdateAccountCredentialsDto>()))
            .ReturnsAsync(_updateCredentialsResponse);

        return new AccountController(_accountServiceMock.Object);
    }

    // PUT api/Account/credentials

    [Fact]
    public async Task UpdateCredentials_WhenCredentialsAreValid_ReturnsOkWithToken()
    {
        // Arrange
        var dto = AuthTestData.DefaultUpdateAccountCredentialsDto;
        var controller = GetController();

        // Act
        var result = await controller.UpdateCredentials(dto);

        // Assert
        AssertEx.Ok(result, new { token = _updateCredentialsResponse });

        _accountServiceMock.Verify(s => s.UpdateAccountCredentialsAsync(dto), Times.Once);
        _accountServiceMock.VerifyNoOtherCalls();
    }
}
