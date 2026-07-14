using Multitool.Domain.Entities.WorkTimePlanner;

namespace Multitool.Domain.Interfaces;

public interface IWorkDayRepository
{
    Task<List<WorkDay>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<WorkDay?> GetByIdAsync(int id);
    Task AddAsync(WorkDay workDay);
    Task UpdateAsync(WorkDay workDay);
    Task DeleteAsync(int id);
}
