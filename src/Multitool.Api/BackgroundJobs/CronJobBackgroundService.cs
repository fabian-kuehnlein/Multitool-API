using Cronos;

namespace Multitool.Api.BackgroundJobs;

public abstract class CronJobBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly CronExpression _cron;

    protected CronJobBackgroundService(IServiceProvider serviceProvider, ILogger logger, string cronExpression)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cron = CronExpression.Parse(cronExpression);
    }

    protected abstract Task ExecuteJobAsync(IServiceScope scope, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobName = GetType().Name;
        _logger.LogInformation("[{Job}] starting", jobName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await ExecuteJobAsync(scope, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Job}] execution failed", jobName);
            }

            var next = _cron.GetNextOccurrence(DateTime.UtcNow);

            if (!next.HasValue)
            {
                _logger.LogWarning("[{Job}] no next occurrence found, stopping", jobName);
                break;
            }

            var delay = next.Value - DateTime.UtcNow;

            if (delay > TimeSpan.Zero)
            {
                _logger.LogDebug("[{Job}] next run at {NextRun}", jobName, next.Value);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
