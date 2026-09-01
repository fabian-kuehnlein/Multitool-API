using System.Diagnostics;
using Microsoft.Extensions.Options;
using Multitool.Api.Configuration;
using Multitool.Application.Interfaces;

namespace Multitool.Api.BackgroundJobs;

public class CleanupPastEventsCronJob : CronJobBackgroundService
{
    private readonly int _months;
    private readonly ILogger<CleanupPastEventsCronJob> _logger;

    public CleanupPastEventsCronJob(
        IServiceProvider serviceProvider,
        ILogger<CleanupPastEventsCronJob> logger,
        IOptions<CronJobSettings> cronSettings)
        : base(serviceProvider, logger, cronSettings.Value.CleanUpPastEvents)
    {
        _months = cronSettings.Value.CleanUpPastEventsMonths;
        _logger = logger;
    }

    protected override async Task ExecuteJobAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        _logger.LogInformation("CleanUpPastEvents job started for events older than {Months} month(s)", _months);

        var stopwatch = Stopwatch.StartNew();
        var calendarService = scope.ServiceProvider.GetRequiredService<ICalendarService>();
        var deletedCount = await calendarService.DeletePastEventsAsync(_months);

        stopwatch.Stop();
        _logger.LogInformation("CleanUpPastEvents job finished: deleted {DeletedCount} event(s) in {Elapsed}", deletedCount, stopwatch.Elapsed);
    }
}
