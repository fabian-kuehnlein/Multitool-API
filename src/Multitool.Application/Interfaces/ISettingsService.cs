using Multitool.Application.Models.Settings;

namespace Multitool.Application.Interfaces;

public interface ISettingsService
{
    Task<SettingsDto> GetSettingsAsync(int userId);
    Task UpdateSettingAsync(int userId, string module, string key, UpdateSettingDto dto);
}
