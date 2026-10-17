using Multitool.Domain.Entities.WorkTimePlanner;

namespace Multitool.Application.Models.WorkTimePlanner;

public record UpsertWorkDayDto(
    DateTime Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int BreakMinutes,
    bool IsHomeOffice,
    DayStatus Status
);
