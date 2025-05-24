using CalendarApi.Businesslogic.Models;
using CalendarApi.DataAccessLayer.Models;
using CalendarApi.Webapi.Models;

public interface ICalendarEventRepository
{
    Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end);
    Task InsertEventAsync(CreateCalendarEventDAO createEvent);
    // Task<bool> UpdateEventAsync(int eventId, CreateCalendarEvent updateEvent);
    // Task<bool> DeleteEventAsync(int eventId);
    Task<List<Category>> GetCategoriesAsync();
}