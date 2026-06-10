using Cronos;
using Microsoft.Extensions.Options;
using Multitool.Api.Configuration;
using Multitool.Application.Interfaces;

namespace Multitool.Api.BackgroundJobs;

public class CleanupPastEventsService(
    IServiceProvider serviceProvider,
    IOptions<CronJobSettings> cronSettings) : BackgroundService
{
    private readonly CronExpression cron = CronExpression.Parse(cronSettings.Value.CleanUpPastEvents);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var next = cron.GetNextOccurrence(DateTime.UtcNow);

            if (next.HasValue)
            {
                var delay = next.Value - DateTime.UtcNow;

                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, stoppingToken);
            }

            using var scope = serviceProvider.CreateScope();
            var calendarService = scope.ServiceProvider.GetRequiredService<ICalendarService>();

            await calendarService.DeletePastEventsAsync(cronSettings.Value.CleanUpPastEventsMonths);
        }
    }
}
