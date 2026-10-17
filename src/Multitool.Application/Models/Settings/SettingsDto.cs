namespace Multitool.Application.Models.Settings;

/// <summary>
/// Complete settings object returned by GET /api/Settings, grouped by module.
/// </summary>
public record SettingsDto(
    WorktimeSettingsDto Worktime);
