using Microsoft.Extensions.Options;
using Multitool.Api.Configuration;
using Multitool.Application.Interfaces;

namespace Multitool.Api.BackgroundJobs;

public class CleanupPastEventsCronJob : CronJobBackgroundService
{
    private readonly int _months;

    public CleanupPastEventsCronJob(
        IServiceProvider serviceProvider,
        IOptions<CronJobSettings> cronSettings)
        : base(serviceProvider, cronSettings.Value.CleanUpPastEvents)
    {
        _months = cronSettings.Value.CleanUpPastEventsMonths;
    }

    protected override async Task ExecuteJobAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var calendarService = scope.ServiceProvider.GetRequiredService<ICalendarService>();
        await calendarService.DeletePastEventsAsync(_months);
    }
}
