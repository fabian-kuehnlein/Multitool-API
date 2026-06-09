using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;
using Multitool.Domain.Entities.Calendar;

namespace Multitool.Infrastructure.Repositories;

public class CalendarRepository(AppDbContext db) : ICalendarRepository
{
    public async Task<CalendarEvent?> GetByIdAsync(int id)
        => await db.CalendarEvents.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end, string? categories)
    {
        List<int>? categoryIds = null;
        if (!string.IsNullOrWhiteSpace(categories))
        {
            categoryIds = categories
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse)
                .ToList();
        }

        var query = db.CalendarEvents
            .Where(e =>
                (e.StartDateTime < end && (e.EndDateTime == null || e.EndDateTime > start))
                ||
                (e.RecurrenceRule != null && (e.RecurrenceEnd == null || e.RecurrenceEnd >= start))
            );

        if (categoryIds is { Count: > 0 })
        {
            query = query.Where(e => categoryIds.Contains(e.CategoryId));
        }

        return await query.ToListAsync();
    }

    public async Task<List<CalendarEvent>> SearchCalendarEventsAsync(string searchString)
    {
        var pattern = $"%{searchString.Trim()}%";

        var results = await db.CalendarEvents
            .AsNoTracking()
            .Where(e =>
                EF.Functions.ILike(e.Title, pattern)
                ||
                (
                    e.Note != null && EF.Functions.ILike(e.Note, pattern)
                ))
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();

        return results;
    }

    public async Task<long> InsertEventAsync(CalendarEvent entity)
    {
        db.CalendarEvents.Add(entity);
        await db.SaveChangesAsync();

        return entity.Id;
    }

    public async Task UpdateEventAsync(CalendarEvent entity)
    {
        db.CalendarEvents.Update(entity);
        await db.SaveChangesAsync();
    }

    public async Task DeleteEventAsync(int Id)
    {
        await db.CalendarEvents
            .Where(e => e.Id == Id)
            .ExecuteDeleteAsync();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    public Task<List<CalendarEvent>> GetEventsOlderThanAsync(DateTime threshold)
    {
        return db.CalendarEvents
            .Where(e =>
                // Non-recurring events that ended before the threshold
                (string.IsNullOrWhiteSpace(e.RecurrenceRule)&&
                    ((e.EndDateTime ?? e.StartDateTime) < threshold))
                ||
                // Recurring events with a recurrence that end before the threshold
                (!string.IsNullOrWhiteSpace(e.RecurrenceRule) &&
                    e.RecurrenceEnd != null &&
                    e.RecurrenceEnd < threshold)
            )
            .ToListAsync();
    }
}
