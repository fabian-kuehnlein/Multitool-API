using CalendarApi.Businesslogic.Models;
using CalendarApi.DataAccessLayer.Models;
using CalendarApi.Webapi.Models;

public interface ICalendarEventRepository
{
    Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end);
    Task InsertEventAsync(CreateCalendarEventDAO createEvent);
    Task<CalendarEventDAO> UpdateEventAsync(CalendarEventDAO updateEvent);
    Task DeleteEventAsync(int eventId);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<HolidayDAO>> GetHolidaysAsync(string year);
}