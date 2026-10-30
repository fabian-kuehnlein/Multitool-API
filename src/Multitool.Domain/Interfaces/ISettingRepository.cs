using Multitool.Domain.Entities.Settings;

namespace Multitool.Domain.Interfaces;

public interface ISettingRepository
{
    Task<List<Setting>> GetSettingsAsync();
    Task<Setting?> GetSettingByKeyAsync(string key);
    Task CreateSettingAsync(Setting setting);
    Task UpdateSettingAsync(Setting setting);
}
