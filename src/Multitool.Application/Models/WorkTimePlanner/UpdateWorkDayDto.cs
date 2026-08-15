using Multitool.Domain.Entities.WorkTimePlanner;

namespace Multitool.Application.Models.WorkTimePlanner;

public record UpdateWorkDayDto(
    DateTime Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int BreakMinutes,
    bool IsHomeOffice,
    DayStatus Status,
    bool IsLocked
);
