using Cronos;

namespace Multitool.Api.BackgroundJobs;

public abstract class CronJobBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CronExpression _cron;

    protected CronJobBackgroundService(IServiceProvider serviceProvider, string cronExpression)
    {
        _serviceProvider = serviceProvider;
        _cron = CronExpression.Parse(cronExpression);
    }

    protected abstract Task ExecuteJobAsync(IServiceScope scope, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var next = _cron.GetNextOccurrence(DateTime.UtcNow);

            if (next.HasValue)
            {
                var delay = next.Value - DateTime.UtcNow;

                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, stoppingToken);
            }

            using var scope = _serviceProvider.CreateScope();
            await ExecuteJobAsync(scope, stoppingToken);
        }
    }
}
