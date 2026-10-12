using Mapster;
using Moq;
using Multitool.Application.Mappings;
using Multitool.Application.Models.WorkTimePlanner;
using Multitool.Application.Services;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class WorkTimePlannerServiceTests
{
    private readonly Mock<IWorkDayRepository> _workDayRepositoryMock;
    private readonly Mock<IWorkTimeSettingsRepository> _settingsRepositoryMock;

    private List<WorkDay> _getWorkDaysByRangeResponse;
    private WorkDay? _getWorkDayByIdResponse;
    private int _createWorkDayResponse;
    private int _getTotalOvertimeResponse;
    private WorkTimeSettings? _getSettingsResponse;

    private static readonly int ID = WorkTimePlannerTestData.DefaultWorkDay.Id;

    private WorkDay? _createdWorkDay;
    private WorkDay? _deletedWorkDay;
    private WorkTimeSettings? _addedSettings;

    public WorkTimePlannerServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());

        _workDayRepositoryMock = new Mock<IWorkDayRepository>();
        _settingsRepositoryMock = new Mock<IWorkTimeSettingsRepository>();

        _getWorkDaysByRangeResponse = new List<WorkDay>();
        _getWorkDayByIdResponse = WorkTimePlannerTestData.DefaultWorkDay;
        _createWorkDayResponse = ID;
        _getTotalOvertimeResponse = 0;
        _getSettingsResponse = WorkTimePlannerTestData.DefaultSettings;
    }

    private WorkTimePlannerService GetService()
    {
        _createdWorkDay = null;
        _deletedWorkDay = null;
        _addedSettings = null;

        _workDayRepositoryMock.Reset();
        _settingsRepositoryMock.Reset();

        _workDayRepositoryMock.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(_getWorkDaysByRangeResponse);

        _workDayRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(_getWorkDayByIdResponse);

        _workDayRepositoryMock.Setup(r => r.GetTotalOvertimeAsync())
            .ReturnsAsync(_getTotalOvertimeResponse);

        _workDayRepositoryMock.Setup(r => r.CreateWorkDayAsync(It.IsAny<WorkDay>()))
            .Callback<WorkDay>(w => _createdWorkDay = w)
            .ReturnsAsync(_createWorkDayResponse);

        _workDayRepositoryMock.Setup(r => r.UpdateWorkDayAsync(It.IsAny<WorkDay>()))
            .Returns(Task.CompletedTask);

        _workDayRepositoryMock.Setup(r => r.DeleteWorkDayAsync(It.IsAny<WorkDay>()))
            .Callback<WorkDay>(w => _deletedWorkDay = w)
            .Returns(Task.CompletedTask);

        _settingsRepositoryMock.Setup(r => r.GetAsync())
            .ReturnsAsync(_getSettingsResponse);

        _settingsRepositoryMock.Setup(r => r.AddAsync(It.IsAny<WorkTimeSettings>()))
            .Callback<WorkTimeSettings>(s => _addedSettings = s)
            .Returns(Task.CompletedTask);

        _settingsRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<WorkTimeSettings>()))
            .Returns(Task.CompletedTask);

        return new WorkTimePlannerService(
            _workDayRepositoryMock.Object,
            _settingsRepositoryMock.Object);
    }

    // GetWorkDaysAsync
    [Fact]
    public async Task GetWorkDaysAsync_WhenWorkDaysExist_ReturnsMappedWorkDays()
    {
        // Arrange
        var workDays = new List<WorkDay> { WorkTimePlannerTestData.DefaultWorkDay };
        _getWorkDaysByRangeResponse = workDays;
        var service = GetService();

        // Act
        var result = await service.GetWorkDaysAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

        // Assert
        AssertEx.AreEqual(result, workDays.Adapt<List<WorkDayDto>>());

        _workDayRepositoryMock.Verify(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();
    }

    // GetWorkDayByIdAsync
    [Fact]
    public async Task GetWorkDayByIdAsync_WhenWorkDayExists_ReturnsWorkDay()
    {
        // Arrange
        var workDay = WorkTimePlannerTestData.DefaultWorkDay;
        _getWorkDayByIdResponse = workDay;
        var service = GetService();

        // Act
        var result = await service.GetWorkDayByIdAsync(ID);

        // Assert
        AssertEx.AreEqual(result, workDay.Adapt<WorkDayDto>());

        _workDayRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetWorkDayByIdAsync_WhenWorkDayDoesNotExist_ReturnsNull()
    {
        // Arrange
        _getWorkDayByIdResponse = null;
        var service = GetService();

        // Act
        var result = await service.GetWorkDayByIdAsync(ID);

        // Assert
        AssertEx.AreEqual(null, result);

        _workDayRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();
    }

    // CreateWorkDayAsync
    [Fact]
    public async Task CreateWorkDayAsync_WhenStartAndEndTimeProvided_CalculatesWorkAndOvertimeMinutes()
    {
        // Arrange
        var dto = WorkTimePlannerTestData.DefaultCreateWorkDayDto;
        var service = GetService();

        // Act
        var result = await service.CreateWorkDayAsync(dto);

        // Assert
        AssertEx.AreEqual(result, ID);
        AssertEx.AreEqual(480, _createdWorkDay!.WorkMinutes);
        AssertEx.AreEqual(0, _createdWorkDay.OvertimeMinutes);

        _workDayRepositoryMock.Verify(r => r.CreateWorkDayAsync(It.Is<WorkDay>(w =>
            w.Date == dto.Date &&
            w.StartTime == dto.StartTime &&
            w.EndTime == dto.EndTime &&
            w.BreakMinutes == dto.BreakMinutes &&
            w.Status == DayStatus.Normal &&
            w.WorkMinutes == 480 &&
            w.OvertimeMinutes == 0
        )), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsRepositoryMock.Verify(r => r.GetAsync(), Times.Once);
        _settingsRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateWorkDayAsync_WhenStatusIsHoliday_SetsWorkAndOvertimeToZero()
    {
        // Arrange
        var dto = WorkTimePlannerTestData.DefaultCreateWorkDayDto with { Status = DayStatus.Holiday };
        var service = GetService();

        // Act
        var result = await service.CreateWorkDayAsync(dto);

        // Assert
        AssertEx.AreEqual(result, ID);
        AssertEx.AreEqual(0, _createdWorkDay!.WorkMinutes);
        AssertEx.AreEqual(0, _createdWorkDay.OvertimeMinutes);

        _workDayRepositoryMock.Verify(r => r.CreateWorkDayAsync(It.Is<WorkDay>(w =>
            w.WorkMinutes == 0 &&
            w.OvertimeMinutes == 0
        )), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsRepositoryMock.Verify(r => r.GetAsync(), Times.Once);
        _settingsRepositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(DayStatus.Vacation)]
    [InlineData(DayStatus.Sick)]
    public async Task CreateWorkDayAsync_WhenStatusIsVacationOrSick_SetsWorkAndOvertimeToZero(DayStatus status)
    {
        // Arrange
        var dto = WorkTimePlannerTestData.DefaultCreateWorkDayDto with { Status = status };
        var service = GetService();

        // Act
        var result = await service.CreateWorkDayAsync(dto);

        // Assert
        AssertEx.AreEqual(result, ID);
        AssertEx.AreEqual(0, _createdWorkDay!.WorkMinutes);
        AssertEx.AreEqual(0, _createdWorkDay.OvertimeMinutes);

        _workDayRepositoryMock.Verify(r => r.CreateWorkDayAsync(It.Is<WorkDay>(w =>
            w.WorkMinutes == 0 &&
            w.OvertimeMinutes == 0
        )), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsRepositoryMock.Verify(r => r.GetAsync(), Times.Once);
        _settingsRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateWorkDayAsync_WhenStartOrEndTimeIsMissing_SetsWorkAndOvertimeToZero()
    {
        // Arrange
        var dto = WorkTimePlannerTestData.DefaultCreateWorkDayDto with { StartTime = null, EndTime = null };
        var service = GetService();

        // Act
        var result = await service.CreateWorkDayAsync(dto);

        // Assert
        AssertEx.AreEqual(result, ID);
        AssertEx.AreEqual(0, _createdWorkDay!.WorkMinutes);
        AssertEx.AreEqual(0, _createdWorkDay.OvertimeMinutes);

        _workDayRepositoryMock.Verify(r => r.CreateWorkDayAsync(It.Is<WorkDay>(w =>
            w.WorkMinutes == 0 &&
            w.OvertimeMinutes == 0
        )), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsRepositoryMock.Verify(r => r.GetAsync(), Times.Once);
        _settingsRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateWorkDayAsync_WhenSettingsDoNotExist_CreatesDefaultSettings()
    {
        // Arrange
        var dto = WorkTimePlannerTestData.DefaultCreateWorkDayDto with { StartTime = null, EndTime = null };
        _getSettingsResponse = null;
        var service = GetService();

        // Act
        await service.CreateWorkDayAsync(dto);

        // Assert
        AssertEx.AreEqual(_addedSettings, new WorkTimeSettings
        {
            DailyTargetMinutes = 480,
            BreakRule6h = 30,
            BreakRule9h = 45,
            HomeOfficeLimit = 20
        });

        _workDayRepositoryMock.Verify(r => r.CreateWorkDayAsync(It.Is<WorkDay>(w =>
            w.WorkMinutes == 0 &&
            w.OvertimeMinutes == 0
        )), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsRepositoryMock.Verify(r => r.GetAsync(), Times.Once);
        _settingsRepositoryMock.Verify(r => r.AddAsync(It.Is<WorkTimeSettings>(s =>
            s.DailyTargetMinutes == 480 &&
            s.BreakRule6h == 30 &&
            s.BreakRule9h == 45 &&
            s.HomeOfficeLimit == 20
        )), Times.Once);
        _settingsRepositoryMock.VerifyNoOtherCalls();
    }

    // UpdateWorkDayAsync
    [Fact]
    public async Task UpdateWorkDayAsync_WhenWorkDayExists_UpdatesAllFields()
    {
        // Arrange
        var existing = WorkTimePlannerTestData.DefaultWorkDay;
        var dto = WorkTimePlannerTestData.DefaultUpdateWorkDayDto;

        _getWorkDayByIdResponse = existing;
        var service = GetService();

        // Act
        await service.UpdateWorkDayAsync(ID, dto);

        // Assert
        AssertEx.AreEqual(dto.Date, existing.Date);
        AssertEx.AreEqual(dto.StartTime, existing.StartTime);
        AssertEx.AreEqual(dto.EndTime, existing.EndTime);
        AssertEx.AreEqual(dto.BreakMinutes, existing.BreakMinutes);
        AssertEx.AreEqual(dto.IsHomeOffice, existing.IsHomeOffice);
        AssertEx.AreEqual(dto.Status, existing.Status);
        AssertEx.AreEqual(dto.IsLocked, existing.IsLocked);
        AssertEx.AreEqual(480, existing.WorkMinutes);
        AssertEx.AreEqual(0, existing.OvertimeMinutes);

        _workDayRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _workDayRepositoryMock.Verify(r => r.UpdateWorkDayAsync(It.Is<WorkDay>(w =>
            w.Id == ID &&
            w.Date == dto.Date &&
            w.StartTime == dto.StartTime &&
            w.BreakMinutes == dto.BreakMinutes
        )), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsRepositoryMock.Verify(r => r.GetAsync(), Times.Once);
        _settingsRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateWorkDayAsync_WhenWorkDayDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getWorkDayByIdResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateWorkDayAsync(ID, WorkTimePlannerTestData.DefaultUpdateWorkDayDto);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _workDayRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _workDayRepositoryMock.Verify(r => r.UpdateWorkDayAsync(It.IsAny<WorkDay>()), Times.Never);
        _workDayRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateWorkDayAsync_WhenWorkDayIsLocked_ThrowsInvalidOperationException()
    {
        // Arrange
        var locked = WorkTimePlannerTestData.DefaultWorkDay;
        locked.IsLocked = true;
        _getWorkDayByIdResponse = locked;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateWorkDayAsync(ID, WorkTimePlannerTestData.DefaultUpdateWorkDayDto);

        // Assert
        await AssertEx.Throws<InvalidOperationException>(act);

        _workDayRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _workDayRepositoryMock.Verify(r => r.UpdateWorkDayAsync(It.IsAny<WorkDay>()), Times.Never);
        _workDayRepositoryMock.VerifyNoOtherCalls();
    }

    // DeleteWorkDayAsync
    [Fact]
    public async Task DeleteWorkDayAsync_WhenWorkDayExists_CallsRepositoryDelete()
    {
        // Arrange
        var workDay = WorkTimePlannerTestData.DefaultWorkDay;
        _getWorkDayByIdResponse = workDay;
        var service = GetService();

        // Act
        await service.DeleteWorkDayAsync(ID);

        // Assert
        AssertEx.AreEqual(workDay, _deletedWorkDay);

        _workDayRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _workDayRepositoryMock.Verify(r => r.DeleteWorkDayAsync(It.Is<WorkDay>(w => w.Id == ID)), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteWorkDayAsync_WhenWorkDayDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getWorkDayByIdResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.DeleteWorkDayAsync(ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _workDayRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _workDayRepositoryMock.Verify(r => r.DeleteWorkDayAsync(It.IsAny<WorkDay>()), Times.Never);
        _workDayRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteWorkDayAsync_WhenWorkDayIsLocked_ThrowsInvalidOperationException()
    {
        // Arrange
        var locked = WorkTimePlannerTestData.DefaultWorkDay;
        locked.IsLocked = true;
        _getWorkDayByIdResponse = locked;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.DeleteWorkDayAsync(ID);

        // Assert
        await AssertEx.Throws<InvalidOperationException>(act);

        _workDayRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _workDayRepositoryMock.Verify(r => r.DeleteWorkDayAsync(It.IsAny<WorkDay>()), Times.Never);
        _workDayRepositoryMock.VerifyNoOtherCalls();
    }

    // GetTotalOvertimeAsync
    [Fact]
    public async Task GetTotalOvertimeAsync_WhenWorkDaysExist_ReturnsSummedOvertime()
    {
        // Arrange
        _getTotalOvertimeResponse = 75;
        var service = GetService();

        // Act
        var result = await service.GetTotalOvertimeAsync();

        // Assert
        AssertEx.AreEqual(75, result);

        _workDayRepositoryMock.Verify(r => r.GetTotalOvertimeAsync(), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetTotalOvertimeAsync_WhenNoWorkDaysExist_ReturnsZero()
    {
        // Arrange
        _getTotalOvertimeResponse = 0;
        var service = GetService();

        // Act
        var result = await service.GetTotalOvertimeAsync();

        // Assert
        AssertEx.AreEqual(0, result);

        _workDayRepositoryMock.Verify(r => r.GetTotalOvertimeAsync(), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();
    }

    // GetSettingsAsync
    [Fact]
    public async Task GetSettingsAsync_WhenSettingsExist_ReturnsSettings()
    {
        // Arrange
        var settings = WorkTimePlannerTestData.DefaultSettings;
        _getSettingsResponse = settings;
        var service = GetService();

        // Act
        var result = await service.GetSettingsAsync();

        // Assert
        AssertEx.AreEqual(result, settings.Adapt<WorkTimeSettingsDto>());

        _settingsRepositoryMock.Verify(r => r.GetAsync(), Times.Once);
        _settingsRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetSettingsAsync_WhenSettingsDoNotExist_CreatesDefaultSettings()
    {
        // Arrange
        _getSettingsResponse = null;
        var service = GetService();

        // Act
        var result = await service.GetSettingsAsync();

        // Assert
        AssertEx.AreEqual(480, result.DailyTargetMinutes);
        AssertEx.AreEqual(30, result.BreakRule6h);
        AssertEx.AreEqual(45, result.BreakRule9h);
        AssertEx.AreEqual(20, result.HomeOfficeLimit);
        AssertEx.AreEqual(_addedSettings, new WorkTimeSettings
        {
            DailyTargetMinutes = 480,
            BreakRule6h = 30,
            BreakRule9h = 45,
            HomeOfficeLimit = 20
        });

        _settingsRepositoryMock.Verify(r => r.GetAsync(), Times.Once);
        _settingsRepositoryMock.Verify(r => r.AddAsync(It.Is<WorkTimeSettings>(s =>
            s.DailyTargetMinutes == 480 &&
            s.BreakRule6h == 30 &&
            s.BreakRule9h == 45 &&
            s.HomeOfficeLimit == 20
        )), Times.Once);
        _settingsRepositoryMock.VerifyNoOtherCalls();
    }

    // UpdateSettingsAsync
    [Fact]
    public async Task UpdateSettingsAsync_WhenSettingsExist_UpdatesAllFields()
    {
        // Arrange
        var existing = WorkTimePlannerTestData.DefaultSettings;
        var dto = WorkTimePlannerTestData.DefaultUpdateSettingsDto;
        _getSettingsResponse = existing;
        var service = GetService();

        // Act
        await service.UpdateSettingsAsync(dto);

        // Assert
        AssertEx.AreEqual(dto.DailyTargetMinutes, existing.DailyTargetMinutes);
        AssertEx.AreEqual(dto.BreakRule6h, existing.BreakRule6h);
        AssertEx.AreEqual(dto.BreakRule9h, existing.BreakRule9h);
        AssertEx.AreEqual(dto.HomeOfficeLimit, existing.HomeOfficeLimit);

        _settingsRepositoryMock.Verify(r => r.GetAsync(), Times.Once);
        _settingsRepositoryMock.Verify(r => r.UpdateAsync(It.Is<WorkTimeSettings>(s =>
            s.DailyTargetMinutes == dto.DailyTargetMinutes &&
            s.BreakRule6h == dto.BreakRule6h &&
            s.BreakRule9h == dto.BreakRule9h &&
            s.HomeOfficeLimit == dto.HomeOfficeLimit
        )), Times.Once);
        _settingsRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateSettingsAsync_WhenSettingsDoNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getSettingsResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateSettingsAsync(WorkTimePlannerTestData.DefaultUpdateSettingsDto);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _settingsRepositoryMock.Verify(r => r.GetAsync(), Times.Once);
        _settingsRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<WorkTimeSettings>()), Times.Never);
        _settingsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<WorkTimeSettings>()), Times.Never);
        _settingsRepositoryMock.VerifyNoOtherCalls();
    }

    // GetHomeOfficeDaysCountAsync
    [Fact]
    public async Task GetHomeOfficeDaysCountAsync_WhenHomeOfficeDaysExist_ReturnsCorrectCount()
    {
        // Arrange
        var homeOfficeDay1 = WorkTimePlannerTestData.DefaultWorkDay;
        homeOfficeDay1.IsHomeOffice = true;
        var homeOfficeDay2 = WorkTimePlannerTestData.DefaultWorkDay;
        homeOfficeDay2.IsHomeOffice = true;
        var officeDay = WorkTimePlannerTestData.DefaultWorkDay;
        officeDay.IsHomeOffice = false;

        _getWorkDaysByRangeResponse = new List<WorkDay> { homeOfficeDay1, homeOfficeDay2, officeDay };
        var service = GetService();

        // Act
        var result = await service.GetHomeOfficeDaysCountAsync(2026, 6);

        // Assert
        AssertEx.AreEqual(2, result);

        _workDayRepositoryMock.Verify(r => r.GetByDateRangeAsync(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 7, 1)
        ), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetHomeOfficeDaysCountAsync_WhenNoHomeOfficeDays_ReturnsZero()
    {
        // Arrange
        var officeDay1 = WorkTimePlannerTestData.DefaultWorkDay;
        officeDay1.IsHomeOffice = false;
        var officeDay2 = WorkTimePlannerTestData.DefaultWorkDay;
        officeDay2.IsHomeOffice = false;

        _getWorkDaysByRangeResponse = new List<WorkDay> { officeDay1, officeDay2 };
        var service = GetService();

        // Act
        var result = await service.GetHomeOfficeDaysCountAsync(2026, 6);

        // Assert
        AssertEx.AreEqual(0, result);

        _workDayRepositoryMock.Verify(r => r.GetByDateRangeAsync(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 7, 1)
        ), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();
    }
}
