using Multitool.Domain.Entities.Calendar;
using MultitoolApi.WebApi.Models;

namespace Multitool.Application.Interfaces;

public interface ICalendarService
{
    Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories);
    Task<List<EventSearchResponse>> SearchCalendarEventsAsync(string searchString);
    Task<long> InsertEventAsync(CreateCalendarEvent newEvent);
    Task UpdateEventAsync(CalendarEvent updateEvent);
    Task DeleteEventAsync(int eventId);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Holiday>> GetHolidaysAsync(string year);
}