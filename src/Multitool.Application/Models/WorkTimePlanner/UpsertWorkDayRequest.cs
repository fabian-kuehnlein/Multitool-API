using Multitool.Domain.Entities.WorkTimePlanner;

namespace Multitool.Application.Models.WorkTimePlanner;

public record UpsertWorkDayRequest(
    DateTime Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int BreakMinutes,
    bool IsHomeOffice,
    DayStatus Status
);
