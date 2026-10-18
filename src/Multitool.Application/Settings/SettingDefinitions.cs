using System.Globalization;

namespace Multitool.Application.Settings;

/// <summary>
/// Registry of all known settings. Every key defined here is created lazily with its
/// default value when a user requests the settings; unknown keys are rejected on update.
/// </summary>
public static class SettingDefinitions
{
    public const string WorktimeModule = "worktime";

    // WORKTIME

    public static SettingDefinition WorktimeDailyTargetMinutes { get; } = new(
        WorktimeModule,
        "daily_target_minutes",
        SettingValueType.Integer,
        WorktimeMasterData.DefaultDailyTargetMinutes.ToString(CultureInfo.InvariantCulture),
        Min: 1,
        Max: 600);

    public static SettingDefinition WorktimeHomeOfficeLimitMonthly { get; } = new(
        WorktimeModule,
        "home_office_limit_monthly",
        SettingValueType.Integer,
        WorktimeMasterData.DefaultHomeOfficeLimitMonthly.ToString(CultureInfo.InvariantCulture),
        Min: 0,
        Max: 31);

    public static SettingDefinition WorktimeHomeOfficeLimitWeekly { get; } = new(
        WorktimeModule,
        "home_office_limit_weekly",
        SettingValueType.Integer,
        WorktimeMasterData.DefaultHomeOfficeLimitWeekly.ToString(CultureInfo.InvariantCulture),
        Min: 0,
        Max: 7);

    public static IReadOnlyList<SettingDefinition> All { get; } =
    [
        WorktimeDailyTargetMinutes,
        WorktimeHomeOfficeLimitMonthly,
        WorktimeHomeOfficeLimitWeekly
    ];

    /// <summary>
    /// Returns the definition for the given module and key, or null if the key is not defined.
    /// </summary>
    public static SettingDefinition? TryGet(string module, string key) =>
        All.FirstOrDefault(d => d.Module == module && d.Key == key);
}
