namespace Multitool.Application.Settings;

/// <summary>
/// Hardcoded worktime master data (Stammdaten).
/// These values are intentionally NOT stored in the database:
/// the break rules are statutory and only change with a backend adjustment,
/// the remaining values are the defaults for the worktime settings.
/// </summary>
public static class WorktimeMasterData
{
    // Break rules (§ 4 ArbSchG) – statutory, changes require a backend adjustment.
    public const int BreakRule6hMinutes = 30;
    public const int BreakRule9hMinutes = 45;

    // Defaults for the worktime settings stored in the settings table.
    public const int DefaultDailyTargetMinutes = 480;
    public const int DefaultHomeOfficeLimitMonthly = 0;
    public const int DefaultHomeOfficeLimitWeekly = 0;
}
