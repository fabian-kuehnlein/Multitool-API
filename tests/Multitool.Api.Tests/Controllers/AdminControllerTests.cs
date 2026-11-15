using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.User;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Api.Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;

    private List<UserDto> _getUsersResponse;
    private int _createUserResponse;

    private static readonly int USER_ID = AuthTestData.DefaultUser.Id;

    public AdminControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();

        // Default responses
        _getUsersResponse = [AuthTestData.DefaultUserDto];
        _createUserResponse = USER_ID;
    }

    private AdminController GetController()
    {
        _userServiceMock.Reset();

        _userServiceMock.Setup(s => s.GetUsersAsync())
            .ReturnsAsync(_getUsersResponse);

        _userServiceMock.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync(_createUserResponse);

        _userServiceMock.Setup(s => s.ResetPasswordAsync(It.IsAny<int>(), It.IsAny<ResetPasswordDto>()))
            .Returns(Task.CompletedTask);

        _userServiceMock.Setup(s => s.SetUserActiveAsync(It.IsAny<int>(), It.IsAny<SetUserActiveDto>()))
            .Returns(Task.CompletedTask);

        return new AdminController(_userServiceMock.Object);
    }

    // GET api/Admin/users

    [Fact]
    public async Task GetUsers_WhenUsersExist_ReturnsOkWithUsers()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetUsers();

        // Assert
        AssertEx.Ok(result, _getUsersResponse);

        _userServiceMock.Verify(s => s.GetUsersAsync(), Times.Once);
        _userServiceMock.VerifyNoOtherCalls();
    }

    // POST api/Admin/users

    [Fact]
    public async Task CreateUser_WhenDtoIsValid_ReturnsCreatedWithId()
    {
        // Arrange
        var newUser = AuthTestData.DefaultCreateUserDto;
        var controller = GetController();

        // Act
        var result = await controller.CreateUser(newUser);

        // Assert
        AssertEx.Created(result, _createUserResponse);

        _userServiceMock.Verify(s => s.CreateUserAsync(newUser), Times.Once);
        _userServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/Admin/users/{id}/password

    [Fact]
    public async Task ResetPassword_WhenUserExists_ReturnsNoContent()
    {
        // Arrange
        var resetPassword = AuthTestData.DefaultResetPasswordDto;
        var controller = GetController();

        // Act
        var result = await controller.ResetPassword(USER_ID, resetPassword);

        // Assert
        AssertEx.NoContent(result);

        _userServiceMock.Verify(s => s.ResetPasswordAsync(USER_ID, resetPassword), Times.Once);
        _userServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/Admin/users/{id}/active

    [Fact]
    public async Task SetUserActive_WhenUserExists_ReturnsNoContent()
    {
        // Arrange
        var setUserActive = AuthTestData.DefaultSetUserActiveDto;
        var controller = GetController();

        // Act
        var result = await controller.SetUserActive(USER_ID, setUserActive);

        // Assert
        AssertEx.NoContent(result);

        _userServiceMock.Verify(s => s.SetUserActiveAsync(USER_ID, setUserActive), Times.Once);
        _userServiceMock.VerifyNoOtherCalls();
    }
}
