namespace Multitool.Application.Models.WorkTimePlanner;

public record UpdateWorkTimeSettingsDto(
    int DailyTargetMinutes,
    int BreakRule6h,
    int BreakRule9h,
    int HomeOfficeLimit
);
