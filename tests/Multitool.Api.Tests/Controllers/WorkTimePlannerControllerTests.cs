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
    private int _totalOvertimeResponse;
    private int _homeOfficeDaysResponse;

    private static readonly int ID = WorkTimePlannerTestData.DefaultWorkDayDto.Id;
    private static readonly int USER_ID = AuthTestData.DefaultUser.Id;

    public WorkTimePlannerControllerTests()
    {
        _workTimePlannerServiceMock = new Mock<IWorkTimePlannerService>();

        // Default responses
        _getWorkDaysResponse = [WorkTimePlannerTestData.DefaultWorkDayDto];
        _totalOvertimeResponse = 75;
        _homeOfficeDaysResponse = 12;
    }

    private WorkTimePlannerController GetController()
    {
        _workTimePlannerServiceMock.Reset();

        _workTimePlannerServiceMock.Setup(s => s.GetWorkDaysAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(_getWorkDaysResponse);

        _workTimePlannerServiceMock.Setup(s => s.UpsertWorkDayAsync(It.IsAny<int>(), It.IsAny<UpsertWorkDayDto>()))
            .ReturnsAsync(ID);

        _workTimePlannerServiceMock.Setup(s => s.SetWorkDayLockedAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        _workTimePlannerServiceMock.Setup(s => s.DeleteWorkDayAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _workTimePlannerServiceMock.Setup(s => s.GetTotalOvertimeAsync())
            .ReturnsAsync(_totalOvertimeResponse);

        _workTimePlannerServiceMock.Setup(s => s.GetHomeOfficeDaysCountAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(_homeOfficeDaysResponse);

        return new WorkTimePlannerController(_workTimePlannerServiceMock.Object)
        {
            ControllerContext = ControllerContextFactory.Create(USER_ID)
        };
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

    // PUT api/WorkTimePlanner/workdays

    [Fact]
    public async Task UpsertWorkDay_WhenWorkDayIsValid_ReturnsNoContent()
    {
        // Arrange
        var upsertWorkDay = WorkTimePlannerTestData.DefaultUpsertWorkDayDto;
        var controller = GetController();

        // Act
        var result = await controller.UpsertWorkDay(upsertWorkDay);

        // Assert
        AssertEx.NoContent(result);

        _workTimePlannerServiceMock.Verify(s => s.UpsertWorkDayAsync(USER_ID, upsertWorkDay), Times.Once);
        _workTimePlannerServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/WorkTimePlanner/workdays/{id}/lock

    [Fact]
    public async Task SetWorkDayLock_WhenWorkDayExists_ReturnsNoContent()
    {
        // Arrange
        var lockWorkDay = WorkTimePlannerTestData.DefaultLockWorkDayDto;
        var controller = GetController();

        // Act
        var result = await controller.SetWorkDayLock(ID, lockWorkDay);

        // Assert
        AssertEx.NoContent(result);

        _workTimePlannerServiceMock.Verify(s => s.SetWorkDayLockedAsync(ID, lockWorkDay.IsLocked), Times.Once);
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

    // GET api/WorkTimePlanner/overtime-balance

    [Fact]
    public async Task GetTotalOvertime_WhenCalled_ReturnsOkWithTotalOvertime()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetTotalOvertime();

        // Assert
        AssertEx.Ok(result, new { totalOvertime = _totalOvertimeResponse });

        _workTimePlannerServiceMock.Verify(s => s.GetTotalOvertimeAsync(), Times.Once);
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
