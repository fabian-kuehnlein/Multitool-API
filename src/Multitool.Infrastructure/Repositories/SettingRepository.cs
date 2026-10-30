using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Settings;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;

namespace Multitool.Infrastructure.Repositories;

public class SettingRepository(AppDbContext db) : ISettingRepository
{
    public async Task<List<Setting>> GetSettingsAsync()
    {
        return await db.Settings
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Setting?> GetSettingByKeyAsync(string key)
    {
        return await db.Settings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key);
    }

    public async Task CreateSettingAsync(Setting setting)
    {
        db.Settings.Add(setting);
        await db.SaveChangesAsync();
    }

    public async Task UpdateSettingAsync(Setting setting)
    {
        db.Settings.Update(setting);
        await db.SaveChangesAsync();
    }
}
