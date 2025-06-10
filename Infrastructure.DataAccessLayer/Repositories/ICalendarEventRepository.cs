using MultitoolApi.Businesslogic.Models;
using MultitoolApi.DataAccessLayer.Models;

public interface ICalendarEventRepository
{
    Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories);
    Task<List<EventSearchResponse>> SearchCalendarEventsAsync(string searchWord);
    Task InsertEventAsync(CreateCalendarEvent createEvent);
    Task UpdateEventAsync(CalendarEvent updateEvent);
    Task DeleteEventAsync(int eventId);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Holiday>> GetHolidaysAsync(string year);
}