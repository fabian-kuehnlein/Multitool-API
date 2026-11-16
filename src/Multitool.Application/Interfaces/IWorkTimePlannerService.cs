using Multitool.Application.Models.WorkTimePlanner;

namespace Multitool.Application.Interfaces;

public interface IWorkTimePlannerService
{
    Task<List<WorkDayModel>> GetWorkDaysAsync(DateTime startDate, DateTime endDate);
    Task<int> UpsertWorkDayAsync(UpsertWorkDayRequest request);
    Task SetWorkDayLockedAsync(int id, bool isLocked);
    Task DeleteWorkDayAsync(int id);

    Task<int> GetTotalOvertimeAsync();

    Task<int> GetHomeOfficeDaysCountAsync(int year, int month);
}
