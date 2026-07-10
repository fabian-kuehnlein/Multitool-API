namespace Multitool.Api.Configuration;

public class CronJobSettings
{
    // Calendar
    public string CleanUpPastEvents { get; set; } = default!;
    public int CleanUpPastEventsMonths { get; set; } = 3;

    // Todo
    public string CleanUpPastTodos { get; set; } = default!;
    public int CleanUpPastTodosDays { get; set; } = 14;
}
