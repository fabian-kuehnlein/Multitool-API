using CalendarApi.Businesslogic.Models;
using CalendarApi.DataAccessLayer.Models;

public interface ICalendarService
{
    Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end);
    Task InsertEventAsync(CreateCalendarEvent createEvent);
    // Task<bool> UpdateEventAsync(int eventId, CreateCalendarEvent updateEvent);
    // Task<bool> DeleteEventAsync(int eventId);
    Task<List<Category>> GetCategoriesAsync();
}