using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Settings;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;

namespace Multitool.Infrastructure.Repositories;

public class SettingRepository(AppDbContext db) : ISettingRepository
{
    public async Task<List<Setting>> GetSettingsByUserAsync(int userId)
    {
        return await db.Settings
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task<Setting?> GetSettingByKeyAsync(int userId, string key)
    {
        return await db.Settings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Key == key);
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
