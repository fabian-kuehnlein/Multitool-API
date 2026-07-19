using System.Globalization;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class WorkTimePlannerService(
    IWorkDayRepository workDayRepository,
    IWeekSummaryRepository weekSummaryRepository,
    IWorkTimeSettingsRepository settingsRepository) : IWorkTimePlannerService
{
    public async Task<List<WorkDay>> GetWorkDaysAsync(DateTime startDate, DateTime endDate)
    {
        return await workDayRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<WorkDay?> GetWorkDayByIdAsync(int id)
    {
        return await workDayRepository.GetByIdAsync(id);
    }

    public async Task<WorkDay> CreateWorkDayAsync(WorkDay workDay)
    {
        var settings = await GetOrCreateSettingsAsync();
        CalculateWorkDayAsync(workDay, settings);
        await workDayRepository.AddAsync(workDay);
        return workDay;
    }

    public async Task UpdateWorkDayAsync(int id, WorkDay workDay)
    {
        var existing = await workDayRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"WorkDay with ID {id} not found.");

        if (existing.IsLocked)
            throw new InvalidOperationException("Cannot modify a locked WorkDay.");

        var settings = await GetOrCreateSettingsAsync();

        existing.Date = workDay.Date;
        existing.StartTime = workDay.StartTime;
        existing.EndTime = workDay.EndTime;
        existing.BreakMinutes = workDay.BreakMinutes;
        existing.IsHomeOffice = workDay.IsHomeOffice;
        existing.Status = workDay.Status;
        existing.IsLocked = workDay.IsLocked;

        CalculateWorkDayAsync(existing, settings);
        await workDayRepository.UpdateAsync(existing);
    }

    public async Task DeleteWorkDayAsync(int id)
    {
        var existing = await workDayRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"WorkDay with ID {id} not found.");

        if (existing.IsLocked)
            throw new InvalidOperationException("Cannot modify a locked WorkDay.");

        await workDayRepository.DeleteAsync(id);
    }

    public async Task<WeekSummary?> GetWeekSummaryAsync(int year, int weekNumber)
    {
        return await weekSummaryRepository.GetByYearAndWeekAsync(year, weekNumber);
    }

    public async Task<WeekSummary> SaveWeekSummaryAsync(int year, int weekNumber)
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
            return existing;
        }

        var summary = new WeekSummary
        {
            Year = year,
            WeekNumber = weekNumber,
            TotalOvertime = totalOvertime
        };

        await weekSummaryRepository.AddAsync(summary);
        return summary;
    }

    public async Task<WorkTimeSettings> GetSettingsAsync()
    {
        return await GetOrCreateSettingsAsync();
    }

    public async Task UpdateSettingsAsync(WorkTimeSettings settings)
    {
        var existing = await settingsRepository.GetAsync()
            ?? throw new NotFoundException("WorkTimeSettings not found.");

        existing.DailyTargetMinutes = settings.DailyTargetMinutes;
        existing.BreakRule6h = settings.BreakRule6h;
        existing.BreakRule9h = settings.BreakRule9h;
        existing.HomeOfficeLimit = settings.HomeOfficeLimit;

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
