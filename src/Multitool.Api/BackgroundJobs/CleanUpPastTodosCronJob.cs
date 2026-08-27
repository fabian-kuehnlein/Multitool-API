using System.Diagnostics;
using Microsoft.Extensions.Options;
using Multitool.Api.Configuration;
using Multitool.Application.Interfaces;

namespace Multitool.Api.BackgroundJobs;

public class CleanupPastTodosCronJob : CronJobBackgroundService
{
    private readonly int _days;
    private readonly ILogger<CleanupPastTodosCronJob> _logger;

    public CleanupPastTodosCronJob(
        IServiceProvider serviceProvider,
        ILogger<CleanupPastTodosCronJob> logger,
        IOptions<CronJobSettings> cronSettings)
        : base(serviceProvider, cronSettings.Value.CleanUpPastTodos)
    {
        _days = cronSettings.Value.CleanUpPastTodosDays;
        _logger = logger;
    }

    protected override async Task ExecuteJobAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        _logger.LogInformation("CleanUpPastTodos job started for todos older than {Days} day(s)", _days);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var todoService = scope.ServiceProvider.GetRequiredService<ITodoService>();
            var deletedCount = await todoService.DeletePastTodosAsync(_days);

            stopwatch.Stop();
            _logger.LogInformation("CleanUpPastTodos job finished: deleted {DeletedCount} todo(s) in {Elapsed}", deletedCount, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "CleanUpPastTodos job failed after {Elapsed}", stopwatch.Elapsed);
            throw;
        }
    }
}
