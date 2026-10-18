namespace Multitool.Application.Models.Settings;

/// <summary>
/// Worktime settings of the settings table (break rules are not part of this,
/// they are hardcoded in <c>WorktimeMasterData</c>).
/// </summary>
public record WorktimeSettingsDto(
    int DailyTargetMinutes,
    int HomeOfficeLimitMonthly,
    int HomeOfficeLimitWeekly);
