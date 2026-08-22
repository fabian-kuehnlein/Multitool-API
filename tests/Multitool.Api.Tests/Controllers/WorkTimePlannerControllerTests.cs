using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.WorkTimePlanner;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Api.Tests.Controllers;

public class WorkTimePlannerControllerTests
{
    private readonly Mock<IWorkTimePlannerService> _workTimePlannerServiceMock;

    private List<WorkDayDto> _getWorkDaysResponse;
    private int _createWorkDayResponse;
    private WeekSummaryDto _weekSummaryResponse;
    private WorkTimeSettingsDto _settingsResponse;
    private int _homeOfficeDaysResponse;

    private static readonly int ID = WorkTimePlannerTestData.DefaultWorkDayDto.Id;

    public WorkTimePlannerControllerTests()
    {
        _workTimePlannerServiceMock = new Mock<IWorkTimePlannerService>();

        // Default responses
        _getWorkDaysResponse = [WorkTimePlannerTestData.DefaultWorkDayDto];
        _createWorkDayResponse = WorkTimePlannerTestData.DefaultWorkDayDto.Id;
        _weekSummaryResponse = WorkTimePlannerTestData.DefaultWeekSummaryDto;
        _settingsResponse = WorkTimePlannerTestData.DefaultSettingsDto;
        _homeOfficeDaysResponse = 12;
    }

    private WorkTimePlannerController GetController()
    {
        _workTimePlannerServiceMock.Reset();

        _workTimePlannerServiceMock.Setup(s => s.GetWorkDaysAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(_getWorkDaysResponse);

        _workTimePlannerServiceMock.Setup(s => s.CreateWorkDayAsync(It.IsAny<CreateWorkDayDto>()))
            .ReturnsAsync(_createWorkDayResponse);

        _workTimePlannerServiceMock.Setup(s => s.UpdateWorkDayAsync(It.IsAny<int>(), It.IsAny<UpdateWorkDayDto>()))
            .Returns(Task.CompletedTask);

        _workTimePlannerServiceMock.Setup(s => s.DeleteWorkDayAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _workTimePlannerServiceMock.Setup(s => s.GetWeekSummaryAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(_weekSummaryResponse);

        _workTimePlannerServiceMock.Setup(s => s.SaveWeekSummaryAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(_weekSummaryResponse);

        _workTimePlannerServiceMock.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(_settingsResponse);

        _workTimePlannerServiceMock.Setup(s => s.UpdateSettingsAsync(It.IsAny<UpdateWorkTimeSettingsDto>()))
            .Returns(Task.CompletedTask);

        _workTimePlannerServiceMock.Setup(s => s.GetHomeOfficeDaysCountAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(_homeOfficeDaysResponse);

        return new WorkTimePlannerController(_workTimePlannerServiceMock.Object);
    }

    // GET api/WorkTimePlanner/workdays

    [Fact]
    public async Task GetWorkDays_WhenWorkDaysExist_ReturnsOkWithWorkDays()
    {
        // Arrange
        var start = WorkTimePlannerTestData.DefaultWorkDayDto.Date;
        var end = start.AddDays(7);
        var controller = GetController();

        // Act
        var result = await controller.GetWorkDays(start, end);

        // Assert
        AssertEx.Ok(result, _getWorkDaysResponse);

        _workTimePlannerServiceMock.Verify(s => s.GetWorkDaysAsync(start, end), Times.Once);
        _workTimePlannerServiceMock.VerifyNoOtherCalls();
    }

    // POST api/WorkTimePlanner/workdays

    [Fact]
    public async Task CreateWorkDay_WhenWorkDayIsValid_ReturnsCreatedWithId()
    {
        // Arrange
        var newWorkDay = WorkTimePlannerTestData.DefaultCreateWorkDayDto;
        var controller = GetController();

        // Act
        var result = await controller.CreateWorkDay(newWorkDay);

        // Assert
        AssertEx.Created(result, _createWorkDayResponse);

        _workTimePlannerServiceMock.Verify(s => s.CreateWorkDayAsync(newWorkDay), Times.Once);
        _workTimePlannerServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/WorkTimePlanner/workdays/{id}

    [Fact]
    public async Task UpdateWorkDay_WhenWorkDayExists_ReturnsNoContent()
    {
        // Arrange
        var updateWorkDay = WorkTimePlannerTestData.DefaultUpdateWorkDayDto;
        var controller = GetController();

        // Act
        var result = await controller.UpdateWorkDay(ID, updateWorkDay);

        // Assert
        AssertEx.NoContent(result);

        _workTimePlannerServiceMock.Verify(s => s.UpdateWorkDayAsync(ID, updateWorkDay), Times.Once);
        _workTimePlannerServiceMock.VerifyNoOtherCalls();
    }

    // DELETE api/WorkTimePlanner/workdays/{id}

    [Fact]
    public async Task DeleteWorkDay_WhenWorkDayExists_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.DeleteWorkDay(ID);

        // Assert
        AssertEx.NoContent(result);

        _workTimePlannerServiceMock.Verify(s => s.DeleteWorkDayAsync(ID), Times.Once);
        _workTimePlannerServiceMock.VerifyNoOtherCalls();
    }

    // GET api/WorkTimePlanner/weeksummary

    [Fact]
    public async Task GetWeekSummary_WhenSummaryExists_ReturnsOkWithSummary()
    {
        // Arrange
        const int year = 2026;
        const int weekNumber = 23;
        var controller = GetController();

        // Act
        var result = await controller.GetWeekSummary(year, weekNumber);

        // Assert
        AssertEx.Ok(result, _weekSummaryResponse);

        _workTimePlannerServiceMock.Verify(s => s.GetWeekSummaryAsync(year, weekNumber), Times.Once);
        _workTimePlannerServiceMock.VerifyNoOtherCalls();
    }

    // POST api/WorkTimePlanner/weeksummary

    [Fact]
    public async Task SaveWeekSummary_WhenSummaryIsSaved_ReturnsOkWithSummary()
    {
        // Arrange
        const int year = 2026;
        const int weekNumber = 23;
        var controller = GetController();

        // Act
        var result = await controller.SaveWeekSummary(year, weekNumber);

        // Assert
        AssertEx.Ok(result, _weekSummaryResponse);

        _workTimePlannerServiceMock.Verify(s => s.SaveWeekSummaryAsync(year, weekNumber), Times.Once);
        _workTimePlannerServiceMock.VerifyNoOtherCalls();
    }

    // GET api/WorkTimePlanner/settings

    [Fact]
    public async Task GetSettings_WhenSettingsExist_ReturnsOkWithSettings()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetSettings();

        // Assert
        AssertEx.Ok(result, _settingsResponse);

        _workTimePlannerServiceMock.Verify(s => s.GetSettingsAsync(), Times.Once);
        _workTimePlannerServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/WorkTimePlanner/settings

    [Fact]
    public async Task UpdateSettings_WhenSettingsExist_ReturnsNoContent()
    {
        // Arrange
        var updateSettings = WorkTimePlannerTestData.DefaultUpdateSettingsDto;
        var controller = GetController();

        // Act
        var result = await controller.UpdateSettings(updateSettings);

        // Assert
        AssertEx.NoContent(result);

        _workTimePlannerServiceMock.Verify(s => s.UpdateSettingsAsync(updateSettings), Times.Once);
        _workTimePlannerServiceMock.VerifyNoOtherCalls();
    }

    // GET api/WorkTimePlanner/homeoffice

    [Fact]
    public async Task GetHomeOfficeDays_WhenCalled_ReturnsOkWithAnonymousObject()
    {
        // Arrange
        const int year = 2026;
        const int month = 6;
        var controller = GetController();

        // Act
        var result = await controller.GetHomeOfficeDays(year, month);

        // Assert
        AssertEx.Ok(result, new { year, month, homeOfficeDays = _homeOfficeDaysResponse });

        _workTimePlannerServiceMock.Verify(s => s.GetHomeOfficeDaysCountAsync(year, month), Times.Once);
        _workTimePlannerServiceMock.VerifyNoOtherCalls();
    }
}
