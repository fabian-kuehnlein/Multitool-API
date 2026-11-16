using Multitool.Application.Models.Settings;

namespace Multitool.Application.Interfaces;

public interface ISettingsService
{
    Task<SettingsModel> GetSettingsAsync();
    Task UpdateSettingAsync(string module, string key, UpdateSettingRequest request);
}
