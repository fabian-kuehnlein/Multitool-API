using Multitool.Domain.Entities.WorkTimePlanner;

namespace Multitool.Application.Models.WorkTimePlanner;

public class WorkDayDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public int BreakMinutes { get; set; }
    public int WorkMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public bool IsHomeOffice { get; set; }
    public DayStatus Status { get; set; }
    public bool IsLocked { get; set; }
}
