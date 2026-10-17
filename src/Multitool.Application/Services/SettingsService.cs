using System.Globalization;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Settings;
using Multitool.Application.Settings;
using Multitool.Domain.Entities.Settings;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class SettingsService(ISettingRepository settingRepository) : ISettingsService
{
    /// <summary>
    /// Returns the complete settings object for the user. Every defined key that is not
    /// stored yet is created lazily with its default value (per key).
    /// </summary>
    public async Task<SettingsDto> GetSettingsAsync(int userId)
    {
        var storedSettings = await settingRepository.GetSettingsByUserAsync(userId);
        var valuesByFullKey = storedSettings.ToDictionary(s => s.Key, s => s.Value);

        foreach (var definition in SettingDefinitions.All)
        {
            if (valuesByFullKey.ContainsKey(definition.FullKey))
                continue;

            var defaultSetting = CreateSetting(userId, definition.FullKey, definition.DefaultValue);
            await settingRepository.CreateSettingAsync(defaultSetting);

            valuesByFullKey[defaultSetting.Key] = defaultSetting.Value;
        }

        return new SettingsDto(
            Worktime: new WorktimeSettingsDto(
                DailyTargetMinutes: GetIntegerValue(valuesByFullKey, SettingDefinitions.WorktimeDailyTargetMinutes),
                HomeOfficeLimitMonthly: GetIntegerValue(valuesByFullKey, SettingDefinitions.WorktimeHomeOfficeLimitMonthly),
                HomeOfficeLimitWeekly: GetIntegerValue(valuesByFullKey, SettingDefinitions.WorktimeHomeOfficeLimitWeekly)));
    }

    /// <summary>
    /// Validates and stores a single setting value for the user.
    /// </summary>
    public async Task UpdateSettingAsync(int userId, string module, string key, UpdateSettingDto dto)
    {
        var definition = SettingDefinitions.TryGet(module, key)
            ?? throw new NotFoundException($"Setting '{module}_{key}' does not exist.");

        var value = ValidateValue(definition, dto.Value);

        var existingSetting = await settingRepository.GetSettingByKeyAsync(userId, definition.FullKey);

        if (existingSetting is null)
        {
            await settingRepository.CreateSettingAsync(CreateSetting(userId, definition.FullKey, value));
            return;
        }

        existingSetting.Value = value;
        existingSetting.UpdatedAt = DateTime.Now;

        await settingRepository.UpdateSettingAsync(existingSetting);
    }

    private static Setting CreateSetting(int userId, string key, string value) => new()
    {
        UserId = userId,
        Key = key,
        Value = value,
        UpdatedAt = DateTime.Now
    };

    /// <summary>
    /// Retrieves and parses an integer setting value using invariant culture.
    /// </summary>
    private static int GetIntegerValue(Dictionary<string, string> valuesByFullKey, SettingDefinition definition) =>
        int.Parse(valuesByFullKey[definition.FullKey], CultureInfo.InvariantCulture);

    /// <summary>
    /// Validates a raw setting value based on its declared type.
    /// Ensures non‑empty input and applies type‑specific normalization.
    /// </summary>
    private static string ValidateValue(SettingDefinition definition, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"The value for setting '{definition.FullKey}' must not be empty.");

        switch (definition.ValueType)
        {
            case SettingValueType.Integer:
                return ValidateIntegerValue(definition, value);

            case SettingValueType.Boolean:
                if (!bool.TryParse(value, out var booleanValue))
                    throw new ArgumentException($"The value for setting '{definition.FullKey}' must be 'true' or 'false'.");

                return booleanValue.ToString().ToLowerInvariant();

            default:
                return value;
        }
    }

    /// <summary>
    /// Validates that the value is a valid integer and within optional min/max bounds.
    /// </summary>
    private static string ValidateIntegerValue(SettingDefinition definition, string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integerValue))
            throw new ArgumentException($"The value for setting '{definition.FullKey}' must be an integer.");

        if (definition.Min is not null && integerValue < definition.Min)
            throw new ArgumentException($"The value for setting '{definition.FullKey}' must be at least {definition.Min}.");

        if (definition.Max is not null && integerValue > definition.Max)
            throw new ArgumentException($"The value for setting '{definition.FullKey}' must be at most {definition.Max}.");

        return integerValue.ToString(CultureInfo.InvariantCulture);
    }
}
