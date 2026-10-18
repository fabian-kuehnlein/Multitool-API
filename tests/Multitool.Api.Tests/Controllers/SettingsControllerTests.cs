using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Settings;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Api.Tests.Controllers;

public class SettingsControllerTests
{
    private readonly Mock<ISettingsService> _settingsServiceMock;

    private SettingsDto _getSettingsResponse;

    private static readonly int USER_ID = AuthTestData.DefaultUser.Id;
    private static readonly string MODULE = SettingTestData.WorktimeModule;
    private static readonly string KEY = SettingTestData.DailyTargetMinutesKey;

    public SettingsControllerTests()
    {
        _settingsServiceMock = new Mock<ISettingsService>();

        // Default responses
        _getSettingsResponse = SettingTestData.DefaultSettingsDto;
    }

    private SettingsController GetController()
    {
        _settingsServiceMock.Reset();

        _settingsServiceMock.Setup(s => s.GetSettingsAsync(It.IsAny<int>()))
            .ReturnsAsync(_getSettingsResponse);

        _settingsServiceMock.Setup(s => s.UpdateSettingAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UpdateSettingDto>()))
            .Returns(Task.CompletedTask);

        return new SettingsController(_settingsServiceMock.Object)
        {
            ControllerContext = ControllerContextFactory.Create(USER_ID)
        };
    }

    // GET api/Settings

    [Fact]
    public async Task GetSettings_WhenSettingsExist_ReturnsOkWithSettings()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetSettings();

        // Assert
        AssertEx.Ok(result, _getSettingsResponse);

        _settingsServiceMock.Verify(s => s.GetSettingsAsync(USER_ID), Times.Once);
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    // PATCH api/Settings/{module}/{key}

    [Fact]
    public async Task UpdateSetting_WhenSettingIsValid_ReturnsNoContent()
    {
        // Arrange
        var updateSetting = SettingTestData.DefaultUpdateSettingDto;
        var controller = GetController();

        // Act
        var result = await controller.UpdateSetting(MODULE, KEY, updateSetting);

        // Assert
        AssertEx.NoContent(result);

        _settingsServiceMock.Verify(s => s.UpdateSettingAsync(USER_ID, MODULE, KEY, updateSetting), Times.Once);
        _settingsServiceMock.VerifyNoOtherCalls();
    }
}
