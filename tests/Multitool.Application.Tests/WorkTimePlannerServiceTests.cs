using Mapster;
using Moq;
using Multitool.Application.Mappings;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Settings;
using Multitool.Application.Models.WorkTimePlanner;
using Multitool.Application.Services;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class WorkTimePlannerServiceTests
{
    private readonly Mock<IWorkDayRepository> _workDayRepositoryMock;
    private readonly Mock<ISettingsService> _settingsServiceMock;

    private List<WorkDay> _getWorkDaysByRangeResponse;
    private WorkDay? _getWorkDayByIdResponse;
    private WorkDay? _getWorkDayByDateResponse;
    private int _upsertWorkDayResponse;
    private int _getTotalOvertimeResponse;
    private SettingsDto _getSettingsResponse;

    private static readonly int ID = WorkTimePlannerTestData.DefaultWorkDay.Id;
    private static readonly int USER_ID = AuthTestData.DefaultUser.Id;

    private WorkDay? _createdWorkDay;
    private WorkDay? _deletedWorkDay;

    public WorkTimePlannerServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());

        _workDayRepositoryMock = new Mock<IWorkDayRepository>();
        _settingsServiceMock = new Mock<ISettingsService>();

        _getWorkDaysByRangeResponse = new List<WorkDay>();
        _getWorkDayByIdResponse = WorkTimePlannerTestData.DefaultWorkDay;
        _getWorkDayByDateResponse = null;
        _upsertWorkDayResponse = ID;
        _getTotalOvertimeResponse = 0;
        _getSettingsResponse = SettingTestData.DefaultSettingsDto;
    }

    private WorkTimePlannerService GetService()
    {
        _createdWorkDay = null;
        _deletedWorkDay = null;

        _workDayRepositoryMock.Reset();
        _settingsServiceMock.Reset();

        _workDayRepositoryMock.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(_getWorkDaysByRangeResponse);

        _workDayRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(_getWorkDayByIdResponse);

        _workDayRepositoryMock.Setup(r => r.GetWorkDayByDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(_getWorkDayByDateResponse);

        _workDayRepositoryMock.Setup(r => r.GetTotalOvertimeAsync())
            .ReturnsAsync(_getTotalOvertimeResponse);

        _workDayRepositoryMock.Setup(r => r.CreateWorkDayAsync(It.IsAny<WorkDay>()))
            .Callback<WorkDay>(w => _createdWorkDay = w)
            .ReturnsAsync(_upsertWorkDayResponse);

        _workDayRepositoryMock.Setup(r => r.UpdateWorkDayAsync(It.IsAny<WorkDay>()))
            .Returns(Task.CompletedTask);

        _workDayRepositoryMock.Setup(r => r.DeleteWorkDayAsync(It.IsAny<WorkDay>()))
            .Callback<WorkDay>(w => _deletedWorkDay = w)
            .Returns(Task.CompletedTask);

        _settingsServiceMock.Setup(s => s.GetSettingsAsync(It.IsAny<int>()))
            .ReturnsAsync(_getSettingsResponse);

        return new WorkTimePlannerService(
            _workDayRepositoryMock.Object,
            _settingsServiceMock.Object);
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

    // UpsertWorkDayAsync
    [Fact]
    public async Task UpsertWorkDayAsync_WhenWorkDayDoesNotExist_CreatesWorkDayAndCalculatesWorkAndOvertimeMinutes()
    {
        // Arrange
        var dto = WorkTimePlannerTestData.DefaultUpsertWorkDayDto;
        var service = GetService();

        // Act
        var result = await service.UpsertWorkDayAsync(USER_ID, dto);

        // Assert
        AssertEx.AreEqual(result, ID);
        AssertEx.AreEqual(480, _createdWorkDay!.WorkMinutes);
        AssertEx.AreEqual(0, _createdWorkDay.OvertimeMinutes);

        _workDayRepositoryMock.Verify(r => r.GetWorkDayByDateAsync(dto.Date), Times.Once);
        _workDayRepositoryMock.Verify(r => r.CreateWorkDayAsync(It.Is<WorkDay>(w =>
            w.Date == dto.Date &&
            w.StartTime == dto.StartTime &&
            w.EndTime == dto.EndTime &&
            w.BreakMinutes == dto.BreakMinutes &&
            w.IsHomeOffice == dto.IsHomeOffice &&
            w.Status == DayStatus.Normal &&
            w.WorkMinutes == 480 &&
            w.OvertimeMinutes == 0
        )), Times.Once);
        _workDayRepositoryMock.Verify(r => r.UpdateWorkDayAsync(It.IsAny<WorkDay>()), Times.Never);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsServiceMock.Verify(s => s.GetSettingsAsync(USER_ID), Times.Once);
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpsertWorkDayAsync_WhenWorkDayForDateAlreadyExists_UpdatesExisting()
    {
        // Arrange
        var existing = WorkTimePlannerTestData.DefaultWorkDay;
        _getWorkDayByDateResponse = existing;
        var dto = WorkTimePlannerTestData.DefaultUpsertWorkDayDto with { StartTime = new TimeOnly(9, 0) };
        var service = GetService();

        // Act
        var result = await service.UpsertWorkDayAsync(USER_ID, dto);

        // Assert
        AssertEx.AreEqual(result, ID);
        AssertEx.AreEqual(dto.StartTime, existing.StartTime);
        AssertEx.AreEqual(dto.EndTime, existing.EndTime);
        AssertEx.AreEqual(dto.BreakMinutes, existing.BreakMinutes);
        AssertEx.AreEqual(dto.IsHomeOffice, existing.IsHomeOffice);
        AssertEx.AreEqual(dto.Status, existing.Status);
        AssertEx.AreEqual(420, existing.WorkMinutes);
        AssertEx.AreEqual(-60, existing.OvertimeMinutes);

        _workDayRepositoryMock.Verify(r => r.GetWorkDayByDateAsync(dto.Date), Times.Once);
        _workDayRepositoryMock.Verify(r => r.CreateWorkDayAsync(It.IsAny<WorkDay>()), Times.Never);
        _workDayRepositoryMock.Verify(r => r.UpdateWorkDayAsync(It.Is<WorkDay>(w =>
            w.Id == ID &&
            w.StartTime == dto.StartTime &&
            w.EndTime == dto.EndTime &&
            w.BreakMinutes == dto.BreakMinutes &&
            w.WorkMinutes == 420 &&
            w.OvertimeMinutes == -60
        )), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsServiceMock.Verify(s => s.GetSettingsAsync(USER_ID), Times.Once);
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(DayStatus.Holiday)]
    [InlineData(DayStatus.Vacation)]
    [InlineData(DayStatus.Sick)]
    public async Task UpsertWorkDayAsync_WhenStatusIsHolidayVacationOrSick_SetsWorkAndOvertimeToZero(DayStatus status)
    {
        // Arrange
        var dto = WorkTimePlannerTestData.DefaultUpsertWorkDayDto with { Status = status };
        var service = GetService();

        // Act
        var result = await service.UpsertWorkDayAsync(USER_ID, dto);

        // Assert
        AssertEx.AreEqual(result, ID);
        AssertEx.AreEqual(0, _createdWorkDay!.WorkMinutes);
        AssertEx.AreEqual(0, _createdWorkDay.OvertimeMinutes);

        _workDayRepositoryMock.Verify(r => r.GetWorkDayByDateAsync(dto.Date), Times.Once);
        _workDayRepositoryMock.Verify(r => r.CreateWorkDayAsync(It.Is<WorkDay>(w =>
            w.WorkMinutes == 0 &&
            w.OvertimeMinutes == 0
        )), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsServiceMock.Verify(s => s.GetSettingsAsync(It.IsAny<int>()), Times.Never);
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpsertWorkDayAsync_WhenStartOrEndTimeIsMissing_SetsWorkAndOvertimeToZero()
    {
        // Arrange
        var dto = WorkTimePlannerTestData.DefaultUpsertWorkDayDto with { StartTime = null, EndTime = null };
        var service = GetService();

        // Act
        var result = await service.UpsertWorkDayAsync(USER_ID, dto);

        // Assert
        AssertEx.AreEqual(result, ID);
        AssertEx.AreEqual(0, _createdWorkDay!.WorkMinutes);
        AssertEx.AreEqual(0, _createdWorkDay.OvertimeMinutes);

        _workDayRepositoryMock.Verify(r => r.GetWorkDayByDateAsync(dto.Date), Times.Once);
        _workDayRepositoryMock.Verify(r => r.CreateWorkDayAsync(It.Is<WorkDay>(w =>
            w.WorkMinutes == 0 &&
            w.OvertimeMinutes == 0
        )), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsServiceMock.Verify(s => s.GetSettingsAsync(It.IsAny<int>()), Times.Never);
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpsertWorkDayAsync_WhenCustomDailyTargetIsConfigured_UsesItForOvertimeCalculation()
    {
        // Arrange
        _getSettingsResponse = new SettingsDto(new WorktimeSettingsDto(400, 0, 0));
        var dto = WorkTimePlannerTestData.DefaultUpsertWorkDayDto;
        var service = GetService();

        // Act
        var result = await service.UpsertWorkDayAsync(USER_ID, dto);

        // Assert
        AssertEx.AreEqual(result, ID);
        AssertEx.AreEqual(480, _createdWorkDay!.WorkMinutes);
        AssertEx.AreEqual(80, _createdWorkDay.OvertimeMinutes);

        _workDayRepositoryMock.Verify(r => r.GetWorkDayByDateAsync(dto.Date), Times.Once);
        _workDayRepositoryMock.Verify(r => r.CreateWorkDayAsync(It.Is<WorkDay>(w =>
            w.WorkMinutes == 480 &&
            w.OvertimeMinutes == 80
        )), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsServiceMock.Verify(s => s.GetSettingsAsync(USER_ID), Times.Once);
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpsertWorkDayAsync_WhenWorkDayForDateExistsAndIsLocked_ThrowsInvalidOperationException()
    {
        // Arrange
        var locked = WorkTimePlannerTestData.DefaultWorkDay;
        locked.IsLocked = true;
        _getWorkDayByDateResponse = locked;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpsertWorkDayAsync(USER_ID, WorkTimePlannerTestData.DefaultUpsertWorkDayDto);

        // Assert
        await AssertEx.Throws<InvalidOperationException>(act);

        _workDayRepositoryMock.Verify(r => r.GetWorkDayByDateAsync(WorkTimePlannerTestData.DefaultUpsertWorkDayDto.Date), Times.Once);
        _workDayRepositoryMock.Verify(r => r.CreateWorkDayAsync(It.IsAny<WorkDay>()), Times.Never);
        _workDayRepositoryMock.Verify(r => r.UpdateWorkDayAsync(It.IsAny<WorkDay>()), Times.Never);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsServiceMock.Verify(s => s.GetSettingsAsync(It.IsAny<int>()), Times.Never);
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    // SetWorkDayLockedAsync
    [Fact]
    public async Task SetWorkDayLockedAsync_WhenWorkDayIsUnlocked_LocksWorkDay()
    {
        // Arrange
        var existing = WorkTimePlannerTestData.DefaultWorkDay;
        existing.IsLocked = false;
        _getWorkDayByIdResponse = existing;
        var service = GetService();

        // Act
        await service.SetWorkDayLockedAsync(ID, true);

        // Assert
        AssertEx.AreEqual(true, existing.IsLocked);

        _workDayRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _workDayRepositoryMock.Verify(r => r.UpdateWorkDayAsync(It.Is<WorkDay>(w =>
            w.Id == ID &&
            w.IsLocked
        )), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SetWorkDayLockedAsync_WhenWorkDayIsLocked_UnlocksWorkDay()
    {
        // Arrange
        var locked = WorkTimePlannerTestData.DefaultWorkDay;
        locked.IsLocked = true;
        _getWorkDayByIdResponse = locked;
        var service = GetService();

        // Act
        await service.SetWorkDayLockedAsync(ID, false);

        // Assert
        AssertEx.AreEqual(false, locked.IsLocked);

        _workDayRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _workDayRepositoryMock.Verify(r => r.UpdateWorkDayAsync(It.Is<WorkDay>(w =>
            w.Id == ID &&
            !w.IsLocked
        )), Times.Once);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SetWorkDayLockedAsync_WhenWorkDayDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getWorkDayByIdResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.SetWorkDayLockedAsync(ID, true);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _workDayRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _workDayRepositoryMock.Verify(r => r.UpdateWorkDayAsync(It.IsAny<WorkDay>()), Times.Never);
        _workDayRepositoryMock.VerifyNoOtherCalls();

        _settingsServiceMock.VerifyNoOtherCalls();
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

        _settingsServiceMock.VerifyNoOtherCalls();
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

        _settingsServiceMock.VerifyNoOtherCalls();
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

        _settingsServiceMock.VerifyNoOtherCalls();
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
