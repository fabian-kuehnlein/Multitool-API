using Multitool.Application.Models.Calendar;

namespace Multitool.Application.Interfaces;

public interface ICalendarService
{
    Task<List<CalendarEventModel>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories);
    Task<List<EventSearchResponseModel>> SearchCalendarEventsAsync(string searchString);
    Task<long> CreateEventAsync(CreateCalendarEventRequest request);
    Task UpdateEventAsync(int id, UpdateCalendarEventRequest request);
    Task DeleteEventAsync(int id);
    Task<List<HolidayModel>> GetHolidaysAsync(string year);
    Task<int> DeletePastEventsAsync(int months);
    Task<byte[]> GenerateIcsFileAsync(GetIcalRequest request);
}
