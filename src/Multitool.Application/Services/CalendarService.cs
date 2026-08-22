using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Application.Models.Calendar;
using System.Web;

namespace Multitool.Application.Services;

public class CalendarService(ICalendarRepository calendarRepository, ITodoRepository todoRepository, ICalendarApiClient calendarApiClient) : ICalendarService
{
    public async Task<List<CalendarEventDto>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories)
    {
        var events = await calendarRepository.GetEventsByRangeAsync(start, end, categories);

        var eventDtos = events.Adapt<List<CalendarEventDto>>();

        var todos = await todoRepository.GetTodosWithDueDateInRangeAsync(start, end);

        var todoEvents = todos.Select(t => new CalendarEventDto
        {
            Id = $"todo-{t.Id}",
            Title = t.Title,
            Note = t.Description,
            StartDateTime = t.DueDate!.Value,
            EndDateTime = t.DueDate.Value.Date.AddDays(1),
            IsAllDay = true,
            CategoryId = t.CategoryId,
            RecurrenceRule = null,
            RecurrenceEnd = null,
            IsTodo = true
        }).ToList();

        eventDtos.AddRange(todoEvents);

        return eventDtos;
    }

    public async Task<List<EventSearchResponseDto>> SearchCalendarEventsAsync(string searchString)
    {
        var result = await calendarRepository.SearchCalendarEventsAsync(searchString);
        return result.Adapt<List<EventSearchResponseDto>>();
    }

    public async Task<long> CreateEventAsync(CreateCalendarEventDto newEvent)
        => await calendarRepository.CreateEventAsync(newEvent.Adapt<CalendarEvent>());

    public async Task UpdateEventAsync(int id, UpdateCalendarEventDto updateCalendarEventDto)
    {
        var existing = await calendarRepository.GetByIdAsync(id);

        if (existing == null)
            throw new NotFoundException($"Event with Id {id} not found");

        existing.Title = updateCalendarEventDto.Title;
        existing.Note = updateCalendarEventDto.Note;
        existing.StartDateTime = updateCalendarEventDto.StartDateTime;
        existing.EndDateTime = updateCalendarEventDto.EndDateTime;
        existing.IsAllDay = updateCalendarEventDto.IsAllDay;
        existing.CategoryId = updateCalendarEventDto.CategoryId;
        existing.RecurrenceRule = updateCalendarEventDto.RecurrenceRule;
        existing.RecurrenceEnd = updateCalendarEventDto.RecurrenceEnd;

        await calendarRepository.UpdateEventAsync(existing);
    }

    public async Task DeleteEventAsync(int id)
    {
        var exists = await calendarRepository.GetByIdAsync(id);

        if (exists == null)
            throw new NotFoundException($"Event with Id {id} not found");

        await calendarRepository.DeleteEventAsync(id);
    }

    public async Task<List<HolidayDto>> GetHolidaysAsync(string year)
    {
        var holidays = await calendarApiClient.GetHolidaysAsync(year);

        if (holidays.Count == 0)
            throw new NotFoundException($"No holidays found for year {year}");

        return holidays.Adapt<List<HolidayDto>>();
    }

    public Task<string> GetICalLinkAsync(GetICalLinkDto calendarEvent)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);

        var start = calendarEvent.StartDateTime.ToUniversalTime();
        var end = (calendarEvent.EndDateTime ?? calendarEvent.StartDateTime).ToUniversalTime();

        if (end <= start)
            end = end.AddHours(1);

        query["title"] = calendarEvent.Title;
        query["start"] = start.ToString("o");
        query["end"] = end.ToString("o");

        query["description"] = calendarEvent.Note ?? string.Empty;

        return Task.FromResult($"https://api.getcal.link/event.ics?{query}");
    }

    public async Task DeletePastEventsAsync(int months)
    {
        var threshold = DateTime.Now.AddMonths(-months);

        var events = await calendarRepository.GetEventsOlderThanAsync(threshold);

        foreach (var e in events)
        {
            if (string.IsNullOrWhiteSpace(e.RecurrenceRule))
            {
                var dateToCheck = e.EndDateTime ?? e.StartDateTime;

                if (dateToCheck < threshold)
                    await calendarRepository.DeleteEventAsync(e.Id);

                continue;
            }

            if (e.RecurrenceEnd == null)
                continue;

            if (e.RecurrenceEnd < threshold)
                await calendarRepository.DeleteEventAsync(e.Id);
        }
    }
}