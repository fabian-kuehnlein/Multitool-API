using MultitoolApi.Businesslogic.Models;
using MultitoolApi.DataAccessLayer.Models;

public interface ICalendarService
{
    Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end);
    Task InsertEventAsync(CreateCalendarEvent createEvent);
    Task<CalendarEvent> UpdateEventAsync(CalendarEvent updateEvent);
    Task DeleteEventAsync(int eventId);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Holiday>> GetHolidaysAsync(string year);
}