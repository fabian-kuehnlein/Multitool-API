using Multitool.Domain.Entities.Calendar;

namespace Multitool.Application.Interfaces;

public interface ICalendarService
{
    Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories);
    Task<List<EventSearchResponse>> SearchCalendarEventsAsync(string searchString);
    Task<long> InsertEventAsync(CreateCalendarEvent newEvent);
    Task UpdateEventAsync(CalendarEvent calendarEvent);
    Task DeleteEventAsync(int id);
    Task<List<Holiday>> GetHolidaysAsync(string year);
    Task DeletePastEventsAsync(int months);
}