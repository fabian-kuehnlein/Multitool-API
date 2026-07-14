namespace Multitool.Domain.Entities.WorkTimePlanner;

public class WorkTimeSettings
{
    public int Id { get; set; }
    public int DailyTargetMinutes { get; set; } = 480;
    public int BreakRule6h { get; set; } = 30;
    public int BreakRule9h { get; set; } = 45;
    public int HomeOfficeLimit { get; set; } = 20;
}
