using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;
using Multitool.Domain.Entities.Calendar;

namespace Multitool.Infrastructure.Repositories;

public class CalendarRepository(AppDbContext db, HttpClient httpClient) : ICalendarRepository
{
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

    public async Task<List<EventSearchResponse>> SearchCalendarEventsAsync(string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return [];

        var pattern = $"%{searchString.Trim()}%";

        var results = await db.CalendarEvents
            .AsNoTracking()
            .Where(e =>
                EF.Functions.Like(e.Title.ToLower(), pattern.ToLower()) ||
                (e.Note != null && EF.Functions.Like(e.Note.ToLower(), pattern.ToLower())))
            .Select(e => new EventSearchResponse
            {
                EventId       = e.Id,
                EventTitle    = e.Title,
                EventNote     = e.Note,
                StartDateTime = e.StartDateTime
            })
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();

        return results;
    }

    public async Task InsertEventAsync(CalendarEvent entity)
    {
        db.CalendarEvents.Add(entity);
        await db.SaveChangesAsync();
    }

    public async Task UpdateEventAsync(CalendarEvent entity)
    {
        var existing = await db.CalendarEvents
                            .FirstOrDefaultAsync(e => e.Id == entity.Id)
                            ?? throw new KeyNotFoundException("Event not found");

        existing.Title          = entity.Title;
        existing.Note           = entity.Note;
        existing.StartDateTime  = entity.StartDateTime;
        existing.EndDateTime    = entity.EndDateTime;
        existing.IsAllDay       = entity.IsAllDay;
        existing.CategoryId     = entity.CategoryId;
        existing.RecurrenceRule = entity.RecurrenceRule;
        existing.RecurrenceEnd  = entity.RecurrenceEnd;

        await db.SaveChangesAsync();
    }

    public async Task DeleteEventAsync(int Id)
    {
        var deleted = await db.CalendarEvents
            .Where(e => e.Id == Id)
            .ExecuteDeleteAsync();

        if (deleted == 0)
            throw new KeyNotFoundException("Event not found");
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<List<Holiday>> GetHolidaysAsync(string year)
    {
        var url = $"?years={year}&states=by";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<HolidayResponse>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return data?.Feiertage?.Select(item => new Holiday
        {
            Name = item.Fname,
            Date = DateTime.Parse(item.Date)
        }).ToList() ?? new List<Holiday>();
    }
}
