using Multitool.Domain.Entities.WorkTimePlanner;

namespace Multitool.Application.Interfaces;

public interface IWorkTimePlannerService
{
    Task<List<WorkDay>> GetWorkDaysAsync(DateTime startDate, DateTime endDate);
    Task<WorkDay?> GetWorkDayByIdAsync(int id);
    Task<WorkDay> CreateWorkDayAsync(WorkDay workDay);
    Task UpdateWorkDayAsync(int id, WorkDay workDay);
    Task DeleteWorkDayAsync(int id);

    Task<WeekSummary?> GetWeekSummaryAsync(int year, int weekNumber);
    Task<WeekSummary> SaveWeekSummaryAsync(int year, int weekNumber);

    Task<WorkTimeSettings> GetSettingsAsync();
    Task UpdateSettingsAsync(WorkTimeSettings settings);

    Task<int> GetHomeOfficeDaysCountAsync(int year, int month);
}
