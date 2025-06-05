using MultitoolApi.Businesslogic.Models;
using MultitoolApi.DataAccessLayer.Models;
using MultitoolApi.Webapi.Models;

public interface ICalendarEventRepository
{
    Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end);
    Task InsertEventAsync(CreateCalendarEventDAO createEvent);
    Task<CalendarEventDAO> UpdateEventAsync(CalendarEventDAO updateEvent);
    Task DeleteEventAsync(int eventId);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<HolidayDAO>> GetHolidaysAsync(string year);
}