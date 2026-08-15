namespace Multitool.Application.Models.WorkTimePlanner;

public class WorkTimeSettingsDto
{
    public int Id { get; set; }
    public int DailyTargetMinutes { get; set; }
    public int BreakRule6h { get; set; }
    public int BreakRule9h { get; set; }
    public int HomeOfficeLimit { get; set; }
}
