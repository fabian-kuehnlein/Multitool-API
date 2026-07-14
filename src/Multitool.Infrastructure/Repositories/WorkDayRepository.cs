using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;

namespace Multitool.Infrastructure.Repositories;

public class WorkDayRepository(AppDbContext db) : IWorkDayRepository
{
    public async Task<List<WorkDay>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await db.WorkDays
            .AsNoTracking()
            .Where(w => w.Date >= startDate && w.Date < endDate)
            .OrderBy(w => w.Date)
            .ToListAsync();
    }

    public async Task<WorkDay?> GetByIdAsync(int id)
    {
        return await db.WorkDays.FindAsync(id);
    }

    public async Task AddAsync(WorkDay workDay)
    {
        await db.WorkDays.AddAsync(workDay);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(WorkDay workDay)
    {
        db.WorkDays.Update(workDay);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var workDay = await db.WorkDays.FindAsync(id);
        if (workDay is not null)
        {
            db.WorkDays.Remove(workDay);
            await db.SaveChangesAsync();
        }
    }
}
