using Multitool.Domain.Entities.WorkTimePlanner;

namespace Multitool.Domain.Interfaces;

public interface IWorkTimeSettingsRepository
{
    Task<WorkTimeSettings?> GetAsync();
    Task UpdateAsync(WorkTimeSettings settings);
    Task AddAsync(WorkTimeSettings settings);
}
