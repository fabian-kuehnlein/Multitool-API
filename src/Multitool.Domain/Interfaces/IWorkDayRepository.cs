using Multitool.Domain.Entities.WorkTimePlanner;

namespace Multitool.Domain.Interfaces;

public interface IWorkDayRepository
{
    Task<List<WorkDay>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<WorkDay?> GetByIdAsync(int id);
    Task<int> CreateWorkDayAsync(WorkDay workDay);
    Task UpdateWorkDayAsync(WorkDay workDay);
    Task DeleteWorkDayAsync(WorkDay workDay);
}
