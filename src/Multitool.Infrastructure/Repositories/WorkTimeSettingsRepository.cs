using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;

namespace Multitool.Infrastructure.Repositories;

public class WorkTimeSettingsRepository(AppDbContext db) : IWorkTimeSettingsRepository
{
    public async Task<WorkTimeSettings?> GetAsync()
    {
        return await db.WorkTimeSettings
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(WorkTimeSettings settings)
    {
        await db.WorkTimeSettings.AddAsync(settings);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(WorkTimeSettings settings)
    {
        db.WorkTimeSettings.Update(settings);
        await db.SaveChangesAsync();
    }
}
