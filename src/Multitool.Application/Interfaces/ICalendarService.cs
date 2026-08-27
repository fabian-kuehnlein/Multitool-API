using Multitool.Application.Models.Calendar;

namespace Multitool.Application.Interfaces;

public interface ICalendarService
{
    Task<List<CalendarEventDto>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories);
    Task<List<EventSearchResponseDto>> SearchCalendarEventsAsync(string searchString);
    Task<long> CreateEventAsync(CreateCalendarEventDto newEvent);
    Task UpdateEventAsync(int id, UpdateCalendarEventDto dto);
    Task DeleteEventAsync(int id);
    Task<List<HolidayDto>> GetHolidaysAsync(string year);
    Task<int> DeletePastEventsAsync(int months);
    Task<byte[]> GenerateIcsFileAsync(GetIcalDto calendarEvent);
}
