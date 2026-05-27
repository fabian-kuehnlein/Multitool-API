using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class CalendarService(ICalendarRepository repository, ICalendarApiClient apiClient) : ICalendarService
{
    public async Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories)
        => await repository.GetEventsByRangeAsync(start, end, categories);

    public async Task<List<EventSearchResponse>> SearchCalendarEventsAsync(string searchString)
    {
        var result = await repository.SearchCalendarEventsAsync(searchString);
        return result.Adapt<List<EventSearchResponse>>();
    }

    public async Task<long> InsertEventAsync(CreateCalendarEvent newEvent)
        => await repository.InsertEventAsync(newEvent.Adapt<CalendarEvent>());

    public async Task UpdateEventAsync(CalendarEvent calendarEvent)
    {
        var existing = await repository.GetByIdAsync(calendarEvent.Id);

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

        await repository.UpdateEventAsync(existing);
    }

    public async Task DeleteEventAsync(int id)
    {
        var exists = await repository.GetByIdAsync(id);

        if (exists == null)
            throw new NotFoundException($"Event with Id {id} not found");

        await repository.DeleteEventAsync(id);
    }
    public async Task<List<Category>> GetCategoriesAsync()
    {
        var categories = await repository.GetCategoriesAsync();

        if (categories is null || categories.Count <= 0)
            throw new NotFoundException("No categories found");

        return categories;
    }

    public async Task<List<Holiday>> GetHolidaysAsync(string year)
        => await apiClient.GetHolidaysAsync(year);
}