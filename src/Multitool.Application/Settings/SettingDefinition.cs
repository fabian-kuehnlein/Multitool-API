namespace Multitool.Application.Settings;

/// <summary>
/// Describes a known setting: where it belongs (module + key), how its value is
/// interpreted and which default is used when the user has no stored value yet.
/// </summary>
public sealed record SettingDefinition(
    string Module,
    string Key,
    SettingValueType ValueType,
    string DefaultValue,
    int? Min = null,
    int? Max = null)
{
    /// <summary>
    /// The key as it is persisted in the database (e.g. "worktime_daily_target_minutes").
    /// </summary>
    public string FullKey => $"{Module}_{Key}";
}
