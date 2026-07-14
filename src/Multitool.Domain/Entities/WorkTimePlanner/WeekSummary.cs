namespace Multitool.Domain.Entities.WorkTimePlanner;

public class WeekSummary
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int WeekNumber { get; set; }
    public int TotalOvertime { get; set; }
}
