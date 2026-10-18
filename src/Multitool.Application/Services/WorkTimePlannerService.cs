using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.WorkTimePlanner;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class WorkTimePlannerService(
    IWorkDayRepository workDayRepository,
    ISettingsService settingsService) : IWorkTimePlannerService
{
    public async Task<List<WorkDayDto>> GetWorkDaysAsync(DateTime startDate, DateTime endDate)
    {
        var workDays = await workDayRepository.GetByDateRangeAsync(startDate, endDate);
        return workDays.Adapt<List<WorkDayDto>>();
    }

    public async Task<int> UpsertWorkDayAsync(int userId, UpsertWorkDayDto dto)
    {
        var existing = await workDayRepository.GetWorkDayByDateAsync(dto.Date);

        if (existing is not null && existing.IsLocked)
            throw new InvalidOperationException($"WorkDay for {dto.Date:yyyy-MM-dd} is locked. Unlock it before making changes.");

        if (existing is null)
        {
            var workDay = dto.Adapt<WorkDay>();
            await CalculateWorkDayAsync(workDay, userId);

            return await workDayRepository.CreateWorkDayAsync(workDay);
        }

        existing.StartTime = dto.StartTime;
        existing.EndTime = dto.EndTime;
        existing.BreakMinutes = dto.BreakMinutes;
        existing.IsHomeOffice = dto.IsHomeOffice;
        existing.Status = dto.Status;

        await CalculateWorkDayAsync(existing, userId);
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

    public async Task<int> GetHomeOfficeDaysCountAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var workDays = await workDayRepository.GetByDateRangeAsync(startDate, endDate);

        return workDays.Count(w => w.IsHomeOffice);
    }

    private async Task CalculateWorkDayAsync(WorkDay workDay, int userId)
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

        var dailyTargetMinutes = (await settingsService.GetSettingsAsync(userId)).Worktime.DailyTargetMinutes;

        var totalMinutes = (int)(workDay.EndTime.Value.ToTimeSpan() - workDay.StartTime.Value.ToTimeSpan()).TotalMinutes;
        workDay.WorkMinutes = Math.Max(0, totalMinutes - workDay.BreakMinutes);
        workDay.OvertimeMinutes = workDay.WorkMinutes - dailyTargetMinutes;
    }
}
