using FluentAssertions;
using Mapster;
using Moq;
using Multitool.Application.Models.WorkTimePlanner;
using Multitool.Application.Services;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Tests;

public class WorkTimePlannerServiceTests
{
    private readonly Mock<IWorkDayRepository> _workDayRepositoryMock;
    private readonly Mock<IWeekSummaryRepository> _weekSummaryRepositoryMock;
    private readonly Mock<IWorkTimeSettingsRepository> _settingsRepositoryMock;
    private readonly WorkTimePlannerService _sut;

    private static readonly WorkTimeSettings DefaultSettings = new()
    {
        Id = 1,
        DailyTargetMinutes = 480,
        BreakRule6h = 30,
        BreakRule9h = 45,
        HomeOfficeLimit = 20
    };

    public WorkTimePlannerServiceTests()
    {
        _workDayRepositoryMock = new Mock<IWorkDayRepository>();
        _weekSummaryRepositoryMock = new Mock<IWeekSummaryRepository>();
        _settingsRepositoryMock = new Mock<IWorkTimeSettingsRepository>();
        _sut = new WorkTimePlannerService(
            _workDayRepositoryMock.Object,
            _weekSummaryRepositoryMock.Object,
            _settingsRepositoryMock.Object);
    }

    // GetWorkDaysAsync

    [Fact]
    public async Task GetWorkDaysAsync_WhenWorkDaysExist_ReturnsWorkDays()
    {
        // Arrange
        var workDays = new List<WorkDay> { new() { Status = DayStatus.Normal } };
        _workDayRepositoryMock.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(workDays);

        // Act
        var result = await _sut.GetWorkDaysAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

        // Assert
        result.Should().BeEquivalentTo(workDays.Adapt<List<WorkDayDto>>());
    }

    // GetWorkDayByIdAsync

    [Fact]
    public async Task GetWorkDayByIdAsync_WhenWorkDayExists_ReturnsWorkDay()
    {
        // Arrange
        var workDay = new WorkDay { Id = 1, Status = DayStatus.Normal };
        _workDayRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(workDay);

        // Act
        var result = await _sut.GetWorkDayByIdAsync(1);

        // Assert
        result.Should().BeEquivalentTo(workDay.Adapt<WorkDayDto>());
    }

    [Fact]
    public async Task GetWorkDayByIdAsync_WhenWorkDayDoesNotExist_ReturnsNull()
    {
        // Arrange
        _workDayRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((WorkDay?)null);

        // Act
        var result = await _sut.GetWorkDayByIdAsync(99);

        // Assert
        result.Should().BeNull();
    }

    // CreateWorkDayAsync

    [Fact]
    public async Task CreateWorkDayAsync_WhenStartAndEndTimeProvided_CalculatesWorkAndOvertimeMinutes()
    {
        // Arrange
        var dto = new CreateWorkDayDto(
            DateTime.UtcNow,
            new TimeOnly(8, 0),
            new TimeOnly(17, 30),
            30,
            false,
            DayStatus.Normal);
        _settingsRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(DefaultSettings);

        // Act
        var result = await _sut.CreateWorkDayAsync(dto);

        // Assert
        result.WorkMinutes.Should().Be(540);
        result.OvertimeMinutes.Should().Be(60);
    }

    [Fact]
    public async Task CreateWorkDayAsync_WhenStatusIsHoliday_SetsWorkAndOvertimeToZero()
    {
        // Arrange
        var dto = new CreateWorkDayDto(
            DateTime.UtcNow,
            new TimeOnly(8, 0),
            new TimeOnly(16, 30),
            0,
            false,
            DayStatus.Holiday);
        _settingsRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(DefaultSettings);

        // Act
        var result = await _sut.CreateWorkDayAsync(dto);

        // Assert
        result.WorkMinutes.Should().Be(0);
        result.OvertimeMinutes.Should().Be(0);
    }

    [Theory]
    [InlineData(DayStatus.Vacation)]
    [InlineData(DayStatus.Sick)]
    public async Task CreateWorkDayAsync_WhenStatusIsVacationOrSick_SetsWorkAndOvertimeToZero(DayStatus status)
    {
        // Arrange
        var dto = new CreateWorkDayDto(
            DateTime.UtcNow,
            new TimeOnly(8, 0),
            new TimeOnly(16, 30),
            0,
            false,
            status);
        _settingsRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(DefaultSettings);

        // Act
        var result = await _sut.CreateWorkDayAsync(dto);

        // Assert
        result.WorkMinutes.Should().Be(0);
        result.OvertimeMinutes.Should().Be(0);
    }

    [Fact]
    public async Task CreateWorkDayAsync_WhenStartOrEndTimeIsNull_SetsWorkAndOvertimeToZero()
    {
        // Arrange
        var dto = new CreateWorkDayDto(DateTime.UtcNow, null, null, 0, false, DayStatus.Normal);
        _settingsRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(DefaultSettings);

        // Act
        var result = await _sut.CreateWorkDayAsync(dto);

        // Assert
        result.WorkMinutes.Should().Be(0);
        result.OvertimeMinutes.Should().Be(0);
    }

    [Fact]
    public async Task CreateWorkDayAsync_WhenSettingsDoNotExist_CreatesDefaultSettings()
    {
        // Arrange
        var dto = new CreateWorkDayDto(DateTime.UtcNow, null, null, 0, false, DayStatus.Normal);
        _settingsRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync((WorkTimeSettings?)null);

        // Act
        await _sut.CreateWorkDayAsync(dto);

        // Assert
        _settingsRepositoryMock.Verify(r => r.AddAsync(It.Is<WorkTimeSettings>(s =>
            s.DailyTargetMinutes == 480 &&
            s.BreakRule6h == 30 &&
            s.BreakRule9h == 45 &&
            s.HomeOfficeLimit == 20)), Times.Once);
    }

    // UpdateWorkDayAsync

    [Fact]
    public async Task UpdateWorkDayAsync_WhenWorkDayExists_UpdatesAllFields()
    {
        // Arrange
        var existing = new WorkDay { Id = 1, Status = DayStatus.Normal, IsLocked = false };
        var dto = new UpdateWorkDayDto(
            new DateTime(2026, 6, 2),
            new TimeOnly(9, 0),
            new TimeOnly(17, 0),
            45,
            true,
            DayStatus.Normal,
            false);
        _workDayRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _settingsRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(DefaultSettings);

        // Act
        await _sut.UpdateWorkDayAsync(1, dto);

        // Assert
        existing.Date.Should().Be(dto.Date);
        existing.StartTime.Should().Be(dto.StartTime);
        existing.EndTime.Should().Be(dto.EndTime);
        existing.BreakMinutes.Should().Be(dto.BreakMinutes);
        existing.IsHomeOffice.Should().Be(dto.IsHomeOffice);
        _workDayRepositoryMock.Verify(r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task UpdateWorkDayAsync_WhenWorkDayDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _workDayRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((WorkDay?)null);

        // Act
        var act = () => _sut.UpdateWorkDayAsync(99, new UpdateWorkDayDto(
            DateTime.UtcNow, null, null, 0, false, DayStatus.Normal, false));

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*99*");
    }

    [Fact]
    public async Task UpdateWorkDayAsync_WhenWorkDayIsLocked_ThrowsInvalidOperationException()
    {
        // Arrange
        var locked = new WorkDay { Id = 1, Status = DayStatus.Normal, IsLocked = true };
        _workDayRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(locked);

        // Act
        var act = () => _sut.UpdateWorkDayAsync(1, new UpdateWorkDayDto(
            DateTime.UtcNow, null, null, 0, false, DayStatus.Normal, false));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _workDayRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<WorkDay>()), Times.Never);
    }

    // DeleteWorkDayAsync

    [Fact]
    public async Task DeleteWorkDayAsync_WhenWorkDayExists_CallsRepositoryDelete()
    {
        // Arrange
        var workDay = new WorkDay { Id = 1, Status = DayStatus.Normal, IsLocked = false };
        _workDayRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(workDay);

        // Act
        await _sut.DeleteWorkDayAsync(1);

        // Assert
        _workDayRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteWorkDayAsync_WhenWorkDayDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _workDayRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((WorkDay?)null);

        // Act
        var act = () => _sut.DeleteWorkDayAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*99*");
    }

    [Fact]
    public async Task DeleteWorkDayAsync_WhenWorkDayIsLocked_ThrowsInvalidOperationException()
    {
        // Arrange
        var locked = new WorkDay { Id = 1, Status = DayStatus.Normal, IsLocked = true };
        _workDayRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(locked);

        // Act
        var act = () => _sut.DeleteWorkDayAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _workDayRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    // GetWeekSummaryAsync

    [Fact]
    public async Task GetWeekSummaryAsync_WhenSummaryExists_ReturnsSummary()
    {
        // Arrange
        var summary = new WeekSummary { Year = 2026, WeekNumber = 23, TotalOvertime = 60 };
        _weekSummaryRepositoryMock.Setup(r => r.GetByYearAndWeekAsync(2026, 23)).ReturnsAsync(summary);

        // Act
        var result = await _sut.GetWeekSummaryAsync(2026, 23);

        // Assert
        result.Should().BeEquivalentTo(summary.Adapt<WeekSummaryDto>());
    }

    [Fact]
    public async Task GetWeekSummaryAsync_WhenSummaryDoesNotExist_ReturnsNull()
    {
        // Arrange
        _weekSummaryRepositoryMock.Setup(r => r.GetByYearAndWeekAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((WeekSummary?)null);

        // Act
        var result = await _sut.GetWeekSummaryAsync(2026, 23);

        // Assert
        result.Should().BeNull();
    }

    // SaveWeekSummaryAsync

    [Fact]
    public async Task SaveWeekSummaryAsync_WhenNoExistingSummary_CreatesNewSummaryWithCumulativeOvertime()
    {
        // Arrange
        var previousSummary = new WeekSummary { Year = 2026, WeekNumber = 22, TotalOvertime = 60 };
        var workDays = new List<WorkDay>
        {
            new() { Status = DayStatus.Normal, OvertimeMinutes = 30 },
            new() { Status = DayStatus.Normal, OvertimeMinutes = -15 }
        };
        _weekSummaryRepositoryMock.Setup(r => r.GetByYearAndWeekAsync(2026, 23)).ReturnsAsync((WeekSummary?)null);
        _weekSummaryRepositoryMock.Setup(r => r.GetByYearAndWeekAsync(2026, 22)).ReturnsAsync(previousSummary);
        _workDayRepositoryMock.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(workDays);

        // Act
        var result = await _sut.SaveWeekSummaryAsync(2026, 23);

        // Assert
        result.TotalOvertime.Should().Be(75); // 60 (previous) + 30 - 15 (this week)
        _weekSummaryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<WeekSummary>()), Times.Once);
    }

    [Fact]
    public async Task SaveWeekSummaryAsync_WhenExistingSummaryExists_UpdatesInsteadOfCreating()
    {
        // Arrange
        var existing = new WeekSummary { Id = 1, Year = 2026, WeekNumber = 23, TotalOvertime = 0 };
        _weekSummaryRepositoryMock.Setup(r => r.GetByYearAndWeekAsync(2026, 23)).ReturnsAsync(existing);
        _weekSummaryRepositoryMock.Setup(r => r.GetByYearAndWeekAsync(2026, 22)).ReturnsAsync((WeekSummary?)null);
        _workDayRepositoryMock.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<WorkDay> { new() { Status = DayStatus.Normal, OvertimeMinutes = 45 } });

        // Act
        var result = await _sut.SaveWeekSummaryAsync(2026, 23);

        // Assert
        result.TotalOvertime.Should().Be(45);
        _weekSummaryRepositoryMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        _weekSummaryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<WeekSummary>()), Times.Never);
    }

    [Fact]
    public async Task SaveWeekSummaryAsync_WhenWeekIsFirstOfYear_UsesPreviousYearLastWeek()
    {
        // Arrange
        _weekSummaryRepositoryMock.Setup(r => r.GetByYearAndWeekAsync(2026, 1)).ReturnsAsync((WeekSummary?)null);
        _weekSummaryRepositoryMock.Setup(r => r.GetByYearAndWeekAsync(2025, It.IsAny<int>())).ReturnsAsync((WeekSummary?)null);
        _workDayRepositoryMock.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<WorkDay>());

        // Act
        await _sut.SaveWeekSummaryAsync(2026, 1);

        // Assert
        _weekSummaryRepositoryMock.Verify(r => r.GetByYearAndWeekAsync(2025, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task SaveWeekSummaryAsync_WhenNoPreviousSummaryExists_StartsFromZero()
    {
        // Arrange
        _weekSummaryRepositoryMock.Setup(r => r.GetByYearAndWeekAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((WeekSummary?)null);
        _workDayRepositoryMock.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<WorkDay> { new() { Status = DayStatus.Normal, OvertimeMinutes = 30 } });

        // Act
        var result = await _sut.SaveWeekSummaryAsync(2026, 23);

        // Assert
        result.TotalOvertime.Should().Be(30);
    }

    // GetSettingsAsync

    [Fact]
    public async Task GetSettingsAsync_WhenSettingsExist_ReturnsSettings()
    {
        // Arrange
        _settingsRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(DefaultSettings);

        // Act
        var result = await _sut.GetSettingsAsync();

        // Assert
        result.Should().BeEquivalentTo(DefaultSettings.Adapt<WorkTimeSettingsDto>());
    }

    [Fact]
    public async Task GetSettingsAsync_WhenSettingsDoNotExist_CreatesDefaultSettings()
    {
        // Arrange
        _settingsRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync((WorkTimeSettings?)null);

        // Act
        var result = await _sut.GetSettingsAsync();

        // Assert
        result.DailyTargetMinutes.Should().Be(480);
        result.BreakRule6h.Should().Be(30);
        result.BreakRule9h.Should().Be(45);
        result.HomeOfficeLimit.Should().Be(20);
        _settingsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<WorkTimeSettings>()), Times.Once);
    }

    // UpdateSettingsAsync

    [Fact]
    public async Task UpdateSettingsAsync_WhenSettingsExist_UpdatesAllFields()
    {
        // Arrange
        var existing = new WorkTimeSettings { Id = 1, DailyTargetMinutes = 480 };
        var dto = new UpdateWorkTimeSettingsDto(450, 20, 40, 15);
        _settingsRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(existing);

        // Act
        await _sut.UpdateSettingsAsync(dto);

        // Assert
        existing.DailyTargetMinutes.Should().Be(dto.DailyTargetMinutes);
        existing.BreakRule6h.Should().Be(dto.BreakRule6h);
        existing.BreakRule9h.Should().Be(dto.BreakRule9h);
        existing.HomeOfficeLimit.Should().Be(dto.HomeOfficeLimit);
        _settingsRepositoryMock.Verify(r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WhenSettingsDoNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _settingsRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync((WorkTimeSettings?)null);

        // Act
        var act = () => _sut.UpdateSettingsAsync(new UpdateWorkTimeSettingsDto(480, 30, 45, 20));

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // GetHomeOfficeDaysCountAsync

    [Fact]
    public async Task GetHomeOfficeDaysCountAsync_WhenHomeOfficeDaysExist_ReturnsCorrectCount()
    {
        // Arrange
        var workDays = new List<WorkDay>
        {
            new() { Status = DayStatus.Normal, IsHomeOffice = true },
            new() { Status = DayStatus.Normal, IsHomeOffice = true },
            new() { Status = DayStatus.Normal, IsHomeOffice = false }
        };
        _workDayRepositoryMock.Setup(r => r.GetByDateRangeAsync(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 7, 1)))
            .ReturnsAsync(workDays);

        // Act
        var result = await _sut.GetHomeOfficeDaysCountAsync(2026, 6);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetHomeOfficeDaysCountAsync_WhenNoHomeOfficeDays_ReturnsZero()
    {
        // Arrange
        var workDays = new List<WorkDay>
        {
            new() { Status = DayStatus.Normal, IsHomeOffice = false },
            new() { Status = DayStatus.Normal, IsHomeOffice = false }
        };
        _workDayRepositoryMock.Setup(r => r.GetByDateRangeAsync(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 7, 1)))
            .ReturnsAsync(workDays);

        // Act
        var result = await _sut.GetHomeOfficeDaysCountAsync(2026, 6);

        // Assert
        result.Should().Be(0);
    }
}
