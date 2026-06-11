using Multitool.Domain.Entities.Calendar;
using Multitool.Application.Models.Calendar;

namespace Multitool.Application.Interfaces;

public interface ICalendarService
{
    Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories);
    Task<List<EventSearchResponseDto>> SearchCalendarEventsAsync(string searchString);
    Task<long> InsertEventAsync(CreateCalendarEventDto newEvent);
    Task UpdateEventAsync(CalendarEvent calendarEvent);
    Task DeleteEventAsync(int id);
    Task<List<Holiday>> GetHolidaysAsync(string year);
    Task DeletePastEventsAsync(int months);
}