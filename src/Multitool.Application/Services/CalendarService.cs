using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Application.Models.Calendar;

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

    public async Task<long> InsertEventAsync(CreateCalendarEventDto newEvent)
        => await calendarRepository.InsertEventAsync(newEvent.Adapt<CalendarEvent>());

    public async Task UpdateEventAsync(CalendarEvent calendarEvent)
    {
        var existing = await calendarRepository.GetByIdAsync(calendarEvent.Id);

        if (existing == null)
            throw new NotFoundException($"Event with Id {calendarEvent.Id} not found");

        existing.Title          = calendarEvent.Title;
        existing.Note           = calendarEvent.Note;
        existing.StartDateTime  = calendarEvent.StartDateTime;
        existing.EndDateTime    = calendarEvent.EndDateTime;
        existing.IsAllDay       = calendarEvent.IsAllDay;
        existing.CategoryId     = calendarEvent.CategoryId;
        existing.RecurrenceRule = calendarEvent.RecurrenceRule;
        existing.RecurrenceEnd  = calendarEvent.RecurrenceEnd;

        await calendarRepository.UpdateEventAsync(existing);
    }

    public async Task DeleteEventAsync(int id)
    {
        var exists = await calendarRepository.GetByIdAsync(id);

        if (exists == null)
            throw new NotFoundException($"Event with Id {id} not found");

        await calendarRepository.DeleteEventAsync(id);
    }

    public async Task<List<Holiday>> GetHolidaysAsync(string year)
        => await calendarApiClient.GetHolidaysAsync(year);

    public async Task DeletePastEventsAsync(int months)
    {
        var now = DateTime.Now;
        var threshold = now.AddMonths(-months);

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