using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class CalendarService(ICalendarRepository calendarRepository, ICalendarApiClient calendarApiClient) : ICalendarService
{
    public async Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories)
        => await calendarRepository.GetEventsByRangeAsync(start, end, categories);

    public async Task<List<EventSearchResponse>> SearchCalendarEventsAsync(string searchString)
    {
        var result = await calendarRepository.SearchCalendarEventsAsync(searchString);
        return result.Adapt<List<EventSearchResponse>>();
    }

    public async Task<long> InsertEventAsync(CreateCalendarEvent newEvent)
        => await calendarRepository.InsertEventAsync(newEvent.Adapt<CalendarEvent>());

    public async Task UpdateEventAsync(CalendarEvent calendarEvent)
    {
        var existing = await calendarRepository.GetByIdAsync(calendarEvent.Id);

        if (existing == null)
            throw new NotFoundException($"Event with Id {calendarEvent.Id} not found");

        existing.Title          = calendarEvent.Title;
        existing.Note           = calendarEvent.Note;
        existing.StartDateTime  = calendarEvent.StartDateTime;
        existing.EndDateTime    = calendarEvent.EndDateTime;
        existing.IsAllDay       = calendarEvent.IsAllDay;
        existing.CategoryId     = calendarEvent.CategoryId;
        existing.RecurrenceRule = calendarEvent.RecurrenceRule;
        existing.RecurrenceEnd  = calendarEvent.RecurrenceEnd;

        await calendarRepository.UpdateEventAsync(existing);
    }

    public async Task DeleteEventAsync(int id)
    {
        var exists = await calendarRepository.GetByIdAsync(id);

        if (exists == null)
            throw new NotFoundException($"Event with Id {id} not found");

        await calendarRepository.DeleteEventAsync(id);
    }
    public async Task<List<Category>> GetCategoriesAsync()
    {
        var categories = await calendarRepository.GetCategoriesAsync();

        if (categories is null || categories.Count <= 0)
            throw new NotFoundException("No categories found");

        return categories;
    }

    public async Task<List<Holiday>> GetHolidaysAsync(string year)
        => await calendarApiClient.GetHolidaysAsync(year);

    public async Task DeletePastEventsAsync(int months)
    {
        var now = DateTime.UtcNow;
        var threshold = now.AddMonths(-months);

        var events = await calendarRepository.GetEventsOlderThanAsync(threshold);

        foreach (var e in events)
        {
            if (string.IsNullOrWhiteSpace(e.RecurrenceRule))
            {
                var dateToCheck = e.EndDateTime ?? e.StartDateTime;

                if (dateToCheck < threshold)
                    await calendarRepository.DeleteEventAsync(e.Id);

                continue;
            }

            if (e.RecurrenceEnd == null)
                continue;

            if (e.RecurrenceEnd < threshold)
                await calendarRepository.DeleteEventAsync(e.Id);
        }
    }
}