namespace Multitool.Api.Configuration;

public class CronJobSettings
{
    public string CleanUpPastEvents { get; set; } = default!;
    public int CleanUpPastEventsMonths { get; set; } = 3;
}
