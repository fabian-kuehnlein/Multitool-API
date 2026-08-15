using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.WorkTimePlanner;
using Multitool.Domain.Entities.WorkTimePlanner;

namespace Multitool.Api.Tests;

public class WorkTimePlannerControllerTests
{
    private readonly Mock<IWorkTimePlannerService> _serviceMock;
    private readonly WorkTimePlannerController _sut;

    private static readonly WorkDayDto DefaultWorkDayDto = new()
    {
        Id = 1,
        Date = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        StartTime = new TimeOnly(8, 0),
        EndTime = new TimeOnly(16, 30),
        BreakMinutes = 30,
        WorkMinutes = 450,
        OvertimeMinutes = -30,
        IsHomeOffice = false,
        Status = DayStatus.Normal,
        IsLocked = false
    };

    private static readonly CreateWorkDayDto DefaultCreateWorkDayDto = new(
        new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        new TimeOnly(8, 0),
        new TimeOnly(16, 30),
        30,
        false,
        DayStatus.Normal
    );

    private static readonly UpdateWorkDayDto DefaultUpdateWorkDayDto = new(
        new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        new TimeOnly(8, 0),
        new TimeOnly(16, 30),
        30,
        false,
        DayStatus.Normal,
        false
    );

    private static readonly WeekSummaryDto DefaultWeekSummaryDto = new()
    {
        Id = 1,
        Year = 2026,
        WeekNumber = 23,
        TotalOvertime = 60
    };

    private static readonly WorkTimeSettingsDto DefaultSettingsDto = new()
    {
        Id = 1,
        DailyTargetMinutes = 480,
        BreakRule6h = 30,
        BreakRule9h = 45,
        HomeOfficeLimit = 20
    };

    private static readonly UpdateWorkTimeSettingsDto DefaultUpdateSettingsDto = new(480, 30, 45, 20);

    public WorkTimePlannerControllerTests()
    {
        _serviceMock = new Mock<IWorkTimePlannerService>();
        _sut = new WorkTimePlannerController(_serviceMock.Object);
    }

    // GET api/worktimeplanner/workdays

    [Fact]
    public async Task GetWorkDays_WhenWorkDaysExist_ReturnsOkWithWorkDays()
    {
        // Arrange
        var workDays = new List<WorkDayDto> { DefaultWorkDayDto };
        _serviceMock.Setup(s => s.GetWorkDaysAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(workDays);

        // Act
        var result = await _sut.GetWorkDays(DefaultWorkDayDto.Date, DefaultWorkDayDto.Date.AddDays(7));

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(workDays);
    }

    // POST api/worktimeplanner/workdays

    [Fact]
    public async Task CreateWorkDay_WhenWorkDayIsValid_ReturnsCreatedAtAction()
    {
        // Arrange
        _serviceMock.Setup(s => s.CreateWorkDayAsync(DefaultCreateWorkDayDto)).ReturnsAsync(DefaultWorkDayDto);

        // Act
        var result = await _sut.CreateWorkDay(DefaultCreateWorkDayDto);

        // Assert
        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.Value.Should().BeEquivalentTo(DefaultWorkDayDto);
    }

    [Fact]
    public async Task CreateWorkDay_WhenWorkDayIsValid_SetsCorrectRouteValues()
    {
        // Arrange
        _serviceMock.Setup(s => s.CreateWorkDayAsync(DefaultCreateWorkDayDto)).ReturnsAsync(DefaultWorkDayDto);

        // Act
        var result = await _sut.CreateWorkDay(DefaultCreateWorkDayDto);

        // Assert
        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(_sut.GetWorkDays));
        created.RouteValues!["startDate"].Should().Be(DefaultWorkDayDto.Date);
        created.RouteValues!["endDate"].Should().Be(DefaultWorkDayDto.Date);
    }

    // PUT api/worktimeplanner/workdays/{id}

    [Fact]
    public async Task UpdateWorkDay_WhenWorkDayExists_ReturnsNoContent()
    {
        // Arrange
        _serviceMock.Setup(s => s.UpdateWorkDayAsync(DefaultWorkDayDto.Id, DefaultUpdateWorkDayDto))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateWorkDay(DefaultWorkDayDto.Id, DefaultUpdateWorkDayDto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    // DELETE api/worktimeplanner/workdays/{id}

    [Fact]
    public async Task DeleteWorkDay_WhenWorkDayExists_ReturnsNoContent()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteWorkDayAsync(DefaultWorkDayDto.Id))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteWorkDay(DefaultWorkDayDto.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    // GET api/worktimeplanner/weeksummary

    [Fact]
    public async Task GetWeekSummary_WhenSummaryExists_ReturnsOkWithSummary()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetWeekSummaryAsync(2026, 23)).ReturnsAsync(DefaultWeekSummaryDto);

        // Act
        var result = await _sut.GetWeekSummary(2026, 23);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(DefaultWeekSummaryDto);
    }

    // POST api/worktimeplanner/weeksummary

    [Fact]
    public async Task SaveWeekSummary_WhenSummaryIsSaved_ReturnsOkWithSummary()
    {
        // Arrange
        _serviceMock.Setup(s => s.SaveWeekSummaryAsync(2026, 23)).ReturnsAsync(DefaultWeekSummaryDto);

        // Act
        var result = await _sut.SaveWeekSummary(2026, 23);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(DefaultWeekSummaryDto);
    }

    // GET api/worktimeplanner/settings

    [Fact]
    public async Task GetSettings_WhenSettingsExist_ReturnsOkWithSettings()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetSettingsAsync()).ReturnsAsync(DefaultSettingsDto);

        // Act
        var result = await _sut.GetSettings();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(DefaultSettingsDto);
    }

    // PUT api/worktimeplanner/settings

    [Fact]
    public async Task UpdateSettings_WhenSettingsExist_ReturnsNoContent()
    {
        // Arrange
        _serviceMock.Setup(s => s.UpdateSettingsAsync(DefaultUpdateSettingsDto)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateSettings(DefaultUpdateSettingsDto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    // GET api/worktimeplanner/homeoffice

    [Fact]
    public async Task GetHomeOfficeDays_WhenCalled_ReturnsOkWithAnonymousObject()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetHomeOfficeDaysCountAsync(2026, 6)).ReturnsAsync(12);

        // Act
        var result = await _sut.GetHomeOfficeDays(2026, 6);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new { year = 2026, month = 6, homeOfficeDays = 12 });
    }
}
