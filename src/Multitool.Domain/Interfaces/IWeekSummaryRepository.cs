using Multitool.Domain.Entities.WorkTimePlanner;

namespace Multitool.Domain.Interfaces;

public interface IWeekSummaryRepository
{
    Task<WeekSummary?> GetByYearAndWeekAsync(int year, int weekNumber);
    Task<WeekSummary?> GetPreviousWeekSummaryAsync(int year, int weekNumber);
    Task AddAsync(WeekSummary summary);
    Task UpdateAsync(WeekSummary summary);
}
