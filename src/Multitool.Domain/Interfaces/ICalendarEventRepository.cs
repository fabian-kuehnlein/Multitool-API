using Multitool.Domain.Entities.Calendar;
using MultitoolApi.Businesslogic.Models;
using MultitoolApi.WebApi.Models;

namespace Multitool.Domain.Interfaces;

public interface ICalendarEventRepository
{
    Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories);
    Task<List<EventSearchResponseDTO>> SearchCalendarEventsAsync(string searchString);
    Task InsertEventAsync(CreateCalendarEventDTO createEvent);
    Task UpdateEventAsync(UpdateCalendarEventDTO updateEvent);
    Task DeleteEventAsync(int eventId);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Holiday>> GetHolidaysAsync(string year);
}