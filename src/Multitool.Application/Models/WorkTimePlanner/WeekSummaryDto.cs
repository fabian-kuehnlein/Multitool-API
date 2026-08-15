namespace Multitool.Application.Models.WorkTimePlanner;

public class WeekSummaryDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int WeekNumber { get; set; }
    public int TotalOvertime { get; set; }
}
