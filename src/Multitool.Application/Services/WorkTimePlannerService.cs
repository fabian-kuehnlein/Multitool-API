using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.WorkTimePlanner;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class WorkTimePlannerService(
    IWorkDayRepository workDayRepository,
    IWorkTimeSettingsRepository settingsRepository) : IWorkTimePlannerService
{
    public async Task<List<WorkDayDto>> GetWorkDaysAsync(DateTime startDate, DateTime endDate)
    {
        var workDays = await workDayRepository.GetByDateRangeAsync(startDate, endDate);
        return workDays.Adapt<List<WorkDayDto>>();
    }

    public async Task<int> UpsertWorkDayAsync(UpsertWorkDayDto dto)
    {
        var existing = await workDayRepository.GetWorkDayByDateAsync(dto.Date);

        if (existing is not null && existing.IsLocked)
            throw new InvalidOperationException($"WorkDay for {dto.Date:yyyy-MM-dd} is locked. Unlock it before making changes.");

        var settings = await GetOrCreateSettingsAsync();

        if (existing is null)
        {
            var workDay = dto.Adapt<WorkDay>();
            CalculateWorkDayAsync(workDay, settings);

            return await workDayRepository.CreateWorkDayAsync(workDay);
        }

        existing.StartTime = dto.StartTime;
        existing.EndTime = dto.EndTime;
        existing.BreakMinutes = dto.BreakMinutes;
        existing.IsHomeOffice = dto.IsHomeOffice;
        existing.Status = dto.Status;

        CalculateWorkDayAsync(existing, settings);
        await workDayRepository.UpdateWorkDayAsync(existing);

        return existing.Id;
    }

    public async Task SetWorkDayLockedAsync(int id, bool isLocked)
    {
        var existing = await workDayRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"WorkDay with ID {id} not found.");

        existing.IsLocked = isLocked;
        
        await workDayRepository.UpdateWorkDayAsync(existing);
    }

    public async Task DeleteWorkDayAsync(int id)
    {
        var existing = await workDayRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"WorkDay with ID {id} not found.");

        if (existing.IsLocked)
            throw new InvalidOperationException($"WorkDay with ID {id} is locked. Unlock it before deleting.");

        await workDayRepository.DeleteWorkDayAsync(existing);
    }

    public async Task<int> GetTotalOvertimeAsync()
    {
        return await workDayRepository.GetTotalOvertimeAsync();
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
}
