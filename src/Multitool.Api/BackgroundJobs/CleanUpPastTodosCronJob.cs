using Microsoft.Extensions.Options;
using Multitool.Api.Configuration;
using Multitool.Application.Interfaces;

namespace Multitool.Api.BackgroundJobs;

public class CleanupPastTodosCronJob : CronJobBackgroundService
{
    private readonly int _days;

    public CleanupPastTodosCronJob(
        IServiceProvider serviceProvider,
        IOptions<CronJobSettings> cronSettings)
        : base(serviceProvider, cronSettings.Value.CleanUpPastTodos)
    {
        _days = cronSettings.Value.CleanUpPastTodosDays;
    }

    protected override async Task ExecuteJobAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var todoService = scope.ServiceProvider.GetRequiredService<ITodoService>();
        await todoService.DeletePastTodosAsync(_days);
    }
}
