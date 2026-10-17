using Multitool.Application.Models.WorkTimePlanner;

namespace Multitool.Application.Interfaces;

public interface IWorkTimePlannerService
{
    Task<List<WorkDayDto>> GetWorkDaysAsync(DateTime startDate, DateTime endDate);
    Task<int> UpsertWorkDayAsync(UpsertWorkDayDto dto);
    Task SetWorkDayLockedAsync(int id, bool isLocked);
    Task DeleteWorkDayAsync(int id);

    Task<int> GetTotalOvertimeAsync();

    Task<WorkTimeSettingsDto> GetSettingsAsync();
    Task UpdateSettingsAsync(UpdateWorkTimeSettingsDto dto);

    Task<int> GetHomeOfficeDaysCountAsync(int year, int month);
}
