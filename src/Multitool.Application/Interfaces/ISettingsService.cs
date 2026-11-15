using Multitool.Application.Models.Settings;

namespace Multitool.Application.Interfaces;

public interface ISettingsService
{
    Task<SettingsDto> GetSettingsAsync();
    Task UpdateSettingAsync(string module, string key, UpdateSettingDto dto);
}
