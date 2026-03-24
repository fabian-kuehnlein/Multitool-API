using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;
using Multitool.Domain.Entities.Calendar;
using MultitoolApi.Businesslogic.Models;
using MultitoolApi.WebApi.Models;
using Microsoft.Extensions.Logging;

namespace Multitool.Infrastructure.Repositories;

public class CalendarEventRepository : ICalendarEventRepository
{
    private readonly AppDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CalendarEventRepository> _logger;

    public CalendarEventRepository(AppDbContext db, HttpClient httpClient, ILogger<CalendarEventRepository> logger)
    {
        _db = db;
        _httpClient = httpClient;
        _logger = logger;
    }

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

        var query = _db.CalendarEvents
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

    public async Task<List<EventSearchResponseDTO>> SearchCalendarEventsAsync(string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return [];

        var pattern = $"%{searchString.Trim()}%";

        var results = await _db.CalendarEvents
            .AsNoTracking()
            .Where(e =>
                EF.Functions.Like(e.Title.ToLower(), pattern.ToLower()) ||
                (e.Note != null && EF.Functions.Like(e.Note.ToLower(), pattern.ToLower())))
            .Select(e => new EventSearchResponseDTO
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

    public async Task InsertEventAsync(CreateCalendarEventDTO dto)
    {
        var entity = new CalendarEvent
        {
            Title     = dto.EventTitle,
            Note      = dto.EventNote,
            StartDateTime  = dto.StartDateTime,
            EndDateTime    = dto.EndDateTime,
            IsAllDay       = dto.IsAllDay,
            CategoryId     = dto.CategoryId,
            RecurrenceRule = dto.RecurrenceRule,
            RecurrenceEnd  = dto.RecurrenceEnd
        };

        _db.CalendarEvents.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateEventAsync(UpdateCalendarEventDTO dto)
    {
        var entity = await _db.CalendarEvents
                            .FirstOrDefaultAsync(e => e.Id == dto.EventId);

        if (entity is null)
            throw new KeyNotFoundException("Event not found");

        entity.Title     = dto.EventTitle;
        entity.Note      = dto.EventNote;
        entity.StartDateTime  = dto.StartDateTime;
        entity.EndDateTime    = dto.EndDateTime;
        entity.IsAllDay       = dto.IsAllDay;
        entity.CategoryId     = dto.CategoryId;
        entity.RecurrenceRule = dto.RecurrenceRule;
        entity.RecurrenceEnd  = dto.RecurrenceEnd;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteEventAsync(int eventId)
    {
        var deleted = await _db.CalendarEvents
            .Where(e => e.Id == eventId)
            .ExecuteDeleteAsync();

        if (deleted == 0)
            throw new KeyNotFoundException("Event not found");
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<List<Holiday>> GetHolidaysAsync(string year)
    {
        var url = $"?years={year}&states=by";
        var response = await _httpClient.GetAsync(url);
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
