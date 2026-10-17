using Multitool.Domain.Entities.Settings;

namespace Multitool.Domain.Interfaces;

public interface ISettingRepository
{
    Task<List<Setting>> GetSettingsByUserAsync(int userId);
    Task<Setting?> GetSettingByKeyAsync(int userId, string key);
    Task CreateSettingAsync(Setting setting);
    Task UpdateSettingAsync(Setting setting);
}
