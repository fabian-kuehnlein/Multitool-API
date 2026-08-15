using Multitool.Application.Models.WorkTimePlanner;

namespace Multitool.Application.Interfaces;

public interface IWorkTimePlannerService
{
    Task<List<WorkDayDto>> GetWorkDaysAsync(DateTime startDate, DateTime endDate);
    Task<WorkDayDto?> GetWorkDayByIdAsync(int id);
    Task<WorkDayDto> CreateWorkDayAsync(CreateWorkDayDto dto);
    Task UpdateWorkDayAsync(int id, UpdateWorkDayDto dto);
    Task DeleteWorkDayAsync(int id);

    Task<WeekSummaryDto?> GetWeekSummaryAsync(int year, int weekNumber);
    Task<WeekSummaryDto> SaveWeekSummaryAsync(int year, int weekNumber);

    Task<WorkTimeSettingsDto> GetSettingsAsync();
    Task UpdateSettingsAsync(UpdateWorkTimeSettingsDto dto);

    Task<int> GetHomeOfficeDaysCountAsync(int year, int month);
}
