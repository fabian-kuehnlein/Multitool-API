using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Calendar;
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
        => await repository.UpdateEventAsync(calendarEvent);

    public async Task DeleteEventAsync(int id)
        => await repository.DeleteEventAsync(id);

    public async Task<List<Category>> GetCategoriesAsync()
        => await repository.GetCategoriesAsync();

    public async Task<List<Holiday>> GetHolidaysAsync(string year)
        => await apiClient.GetHolidaysAsync(year);
}