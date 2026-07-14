using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;

namespace Multitool.Infrastructure.Repositories;

public class WeekSummaryRepository(AppDbContext db) : IWeekSummaryRepository
{
    public async Task<WeekSummary?> GetByYearAndWeekAsync(int year, int weekNumber)
    {
        return await db.WeekSummaries
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Year == year && w.WeekNumber == weekNumber);
    }

    public async Task<WeekSummary?> GetPreviousWeekSummaryAsync(int year, int weekNumber)
    {
        return await db.WeekSummaries
            .AsNoTracking()
            .Where(w => (w.Year < year) || (w.Year == year && w.WeekNumber < weekNumber))
            .OrderByDescending(w => w.Year)
            .ThenByDescending(w => w.WeekNumber)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(WeekSummary summary)
    {
        await db.WeekSummaries.AddAsync(summary);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(WeekSummary summary)
    {
        db.WeekSummaries.Update(summary);
        await db.SaveChangesAsync();
    }
}
