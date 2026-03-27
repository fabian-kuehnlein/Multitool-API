using Multitool.Domain.Entities.Calendar;

namespace Multitool.Domain.Interfaces;

public interface ICalendarRepository
{
    Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories);
    Task<List<CalendarEvent>> SearchCalendarEventsAsync(string searchString);
    Task<long> InsertEventAsync(CalendarEvent createEvent);
    Task UpdateEventAsync(CalendarEvent updateEvent);
    Task DeleteEventAsync(int Id);
    Task<List<Category>> GetCategoriesAsync();
}