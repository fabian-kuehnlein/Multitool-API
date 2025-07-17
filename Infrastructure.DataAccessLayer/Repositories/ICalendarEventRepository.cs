using MultitoolApi.Businesslogic.Models;
using MultitoolApi.DataAccessLayer.Models;
using MultitoolApi.WebApi.Models;

public interface ICalendarEventRepository
{
    Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories);
    Task<List<EventSearchResponseDTO>> SearchCalendarEventsAsync(string searchString);
    Task InsertEventAsync(CreateCalendarEventDTO createEvent);
    Task UpdateEventAsync(UpdateCalendarEventDTO updateEvent);
    Task DeleteEventAsync(int eventId);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Holiday>> GetHolidaysAsync(string year);
}