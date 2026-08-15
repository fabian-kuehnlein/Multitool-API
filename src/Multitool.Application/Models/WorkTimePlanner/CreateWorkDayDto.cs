using Multitool.Domain.Entities.WorkTimePlanner;

namespace Multitool.Application.Models.WorkTimePlanner;

public record CreateWorkDayDto(
    DateTime Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int BreakMinutes,
    bool IsHomeOffice,
    DayStatus Status
);
