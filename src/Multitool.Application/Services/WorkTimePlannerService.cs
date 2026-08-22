using System.Globalization;
using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.WorkTimePlanner;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class WorkTimePlannerService(
    IWorkDayRepository workDayRepository,
    IWeekSummaryRepository weekSummaryRepository,
    IWorkTimeSettingsRepository settingsRepository) : IWorkTimePlannerService
{
    public async Task<List<WorkDayDto>> GetWorkDaysAsync(DateTime startDate, DateTime endDate)
    {
        var workDays = await workDayRepository.GetByDateRangeAsync(startDate, endDate);
        return workDays.Adapt<List<WorkDayDto>>();
    }

    public async Task<WorkDayDto?> GetWorkDayByIdAsync(int id)
    {
        var workDay = await workDayRepository.GetByIdAsync(id);
        return workDay?.Adapt<WorkDayDto>();
    }

    public async Task<int> CreateWorkDayAsync(CreateWorkDayDto dto)
    {
        var workDay = dto.Adapt<WorkDay>();

        var settings = await GetOrCreateSettingsAsync();
        CalculateWorkDayAsync(workDay, settings);
        
        return await workDayRepository.CreateWorkDayAsync(workDay);
    }

    public async Task UpdateWorkDayAsync(int id, UpdateWorkDayDto dto)
    {
        var existing = await workDayRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"WorkDay with ID {id} not found.");

        if (existing.IsLocked)
            throw new InvalidOperationException("Cannot modify a locked WorkDay.");

        var settings = await GetOrCreateSettingsAsync();

        existing.Date = dto.Date;
        existing.StartTime = dto.StartTime;
        existing.EndTime = dto.EndTime;
        existing.BreakMinutes = dto.BreakMinutes;
        existing.IsHomeOffice = dto.IsHomeOffice;
        existing.Status = dto.Status;
        existing.IsLocked = dto.IsLocked;

        CalculateWorkDayAsync(existing, settings);
        
        await workDayRepository.UpdateWorkDayAsync(existing);
    }

    public async Task DeleteWorkDayAsync(int id)
    {
        var existing = await workDayRepository.GetByIdAsync(id);
        
        if (existing == null)
            throw new NotFoundException($"WorkDay with ID {id} not found.");

        if (existing.IsLocked)
            throw new InvalidOperationException("Cannot modify a locked WorkDay.");

        await workDayRepository.DeleteWorkDayAsync(existing);
    }

    public async Task<WeekSummaryDto?> GetWeekSummaryAsync(int year, int weekNumber)
    {
        var summary = await weekSummaryRepository.GetByYearAndWeekAsync(year, weekNumber);
        return summary?.Adapt<WeekSummaryDto>();
    }

    public async Task<WeekSummaryDto> SaveWeekSummaryAsync(int year, int weekNumber)
    {
        var existing = await weekSummaryRepository.GetByYearAndWeekAsync(year, weekNumber);

        var (previousYear, previousWeek) = GetPreviousWeek(year, weekNumber);
        var previousSummary = await weekSummaryRepository.GetByYearAndWeekAsync(previousYear, previousWeek);
        var previousOvertime = previousSummary?.TotalOvertime ?? 0;

        var weekStart = ISOWeek.ToDateTime(year, weekNumber, DayOfWeek.Monday);
        var weekEnd = weekStart.AddDays(7);

        var workDays = await workDayRepository.GetByDateRangeAsync(weekStart, weekEnd);
        var weekOvertime = workDays.Sum(w => w.OvertimeMinutes);

        var totalOvertime = previousOvertime + weekOvertime;

        if (existing is not null)
        {
            existing.TotalOvertime = totalOvertime;
            await weekSummaryRepository.UpdateAsync(existing);
            return existing.Adapt<WeekSummaryDto>();
        }

        var summary = new WeekSummary
        {
            Year = year,
            WeekNumber = weekNumber,
            TotalOvertime = totalOvertime
        };

        await weekSummaryRepository.AddAsync(summary);
        return summary.Adapt<WeekSummaryDto>();
    }

    public async Task<WorkTimeSettingsDto> GetSettingsAsync()
    {
        var settings = await GetOrCreateSettingsAsync();
        return settings.Adapt<WorkTimeSettingsDto>();
    }

    public async Task UpdateSettingsAsync(UpdateWorkTimeSettingsDto dto)
    {
        var existing = await settingsRepository.GetAsync()
            ?? throw new NotFoundException("WorkTimeSettings not found.");

        existing.DailyTargetMinutes = dto.DailyTargetMinutes;
        existing.BreakRule6h = dto.BreakRule6h;
        existing.BreakRule9h = dto.BreakRule9h;
        existing.HomeOfficeLimit = dto.HomeOfficeLimit;

        await settingsRepository.UpdateAsync(existing);
    }

    public async Task<int> GetHomeOfficeDaysCountAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var workDays = await workDayRepository.GetByDateRangeAsync(startDate, endDate);

        return workDays.Count(w => w.IsHomeOffice);
    }

    private async Task<WorkTimeSettings> GetOrCreateSettingsAsync()
    {
        var settings = await settingsRepository.GetAsync();

        if (settings is null)
        {
            settings = new WorkTimeSettings
            {
                DailyTargetMinutes = 480,
                BreakRule6h = 30,
                BreakRule9h = 45,
                HomeOfficeLimit = 20
            };
            await settingsRepository.AddAsync(settings);
        }

        return settings;
    }

    private static void CalculateWorkDayAsync(WorkDay workDay, WorkTimeSettings settings)
    {
        if (workDay.Status == DayStatus.Holiday || workDay.Status == DayStatus.Vacation || workDay.Status == DayStatus.Sick)
        {
            workDay.WorkMinutes = 0;
            workDay.OvertimeMinutes = 0;
            return;
        }

        if (workDay.StartTime is null || workDay.EndTime is null)
        {
            workDay.WorkMinutes = 0;
            workDay.OvertimeMinutes = 0;
            return;
        }

        var totalMinutes = (int)(workDay.EndTime.Value.ToTimeSpan() - workDay.StartTime.Value.ToTimeSpan()).TotalMinutes;
        workDay.WorkMinutes = Math.Max(0, totalMinutes - workDay.BreakMinutes);
        workDay.OvertimeMinutes = workDay.WorkMinutes - settings.DailyTargetMinutes;
    }

    private static (int Year, int WeekNumber) GetPreviousWeek(int year, int weekNumber)
    {
        if (weekNumber <= 1)
            return (year - 1, GetTotalWeeksOfYear(year - 1));

        return (year, weekNumber - 1);
    }

    private static int GetTotalWeeksOfYear(int year)
    {
        var dec28 = new DateTime(year, 12, 28);
        return ISOWeek.GetWeekOfYear(dec28);
    }
}
