using Multitool.Application.Models.WorkTimePlanner;
using Multitool.Domain.Entities.WorkTimePlanner;

namespace Multitool.Tests.Shared;

public static class WorkTimePlannerTestData
{
    public static WorkDayDto DefaultWorkDayDto => new()
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

    public static CreateWorkDayDto DefaultCreateWorkDayDto => new(
        new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        new TimeOnly(8, 0),
        new TimeOnly(16, 30),
        30,
        false,
        DayStatus.Normal
    );

    public static UpdateWorkDayDto DefaultUpdateWorkDayDto => new(
        new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        new TimeOnly(8, 0),
        new TimeOnly(16, 30),
        30,
        false,
        DayStatus.Normal,
        false
    );

    public static WeekSummaryDto DefaultWeekSummaryDto => new()
    {
        Id = 1,
        Year = 2026,
        WeekNumber = 23,
        TotalOvertime = 60
    };

    public static WorkTimeSettingsDto DefaultSettingsDto => new()
    {
        Id = 1,
        DailyTargetMinutes = 480,
        BreakRule6h = 30,
        BreakRule9h = 45,
        HomeOfficeLimit = 20
    };

    public static UpdateWorkTimeSettingsDto DefaultUpdateSettingsDto => new(480, 30, 45, 20);
}
