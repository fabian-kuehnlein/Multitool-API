using Multitool.Application.Models.Settings;
using Multitool.Domain.Entities.Settings;

namespace Multitool.Tests.Shared;

public static class SettingTestData
{
    // Full keys as they are stored in the database (module_key).
    public const string DailyTargetMinutesFullKey = "worktime_daily_target_minutes";
    public const string HomeOfficeLimitMonthlyFullKey = "worktime_home_office_limit_monthly";
    public const string HomeOfficeLimitWeeklyFullKey = "worktime_home_office_limit_weekly";

    // Route parts for PATCH /api/Settings/{module}/{key}.
    public const string WorktimeModule = "worktime";
    public const string DailyTargetMinutesKey = "daily_target_minutes";

    public static Setting DefaultSetting => new()
    {
        Id = 1,
        UserId = AuthTestData.DefaultUser.Id,
        Key = DailyTargetMinutesFullKey,
        Value = "480",
        UpdatedAt = DateTime.Now
    };

    public static WorktimeSettingsDto DefaultWorktimeSettingsDto => new(
        DailyTargetMinutes: 480,
        HomeOfficeLimitMonthly: 0,
        HomeOfficeLimitWeekly: 0);

    public static SettingsDto DefaultSettingsDto => new(
        Worktime: DefaultWorktimeSettingsDto);

    public static UpdateSettingDto DefaultUpdateSettingDto => new("600");
}
