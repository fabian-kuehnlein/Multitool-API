using System.Text;
using Ical.Net;
using IcalEvent = Ical.Net.CalendarComponents.CalendarEvent;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Mapster;
using Multitool.Application.Extensions;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Application.Models.Calendar;

namespace Multitool.Application.Services;

public class CalendarService(
    ICalendarRepository calendarRepository,
    ITodoRepository todoRepository,
    ICategoryRepository categoryRepository,
    ICalendarApiClient calendarApiClient) : ICalendarService
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
    {
        await categoryRepository.GetApplicableCategoryAsync(newEvent.CategoryId, AppModule.Calendar);

        return await calendarRepository.CreateEventAsync(newEvent.Adapt<CalendarEvent>());
    }

    public async Task UpdateEventAsync(int id, UpdateCalendarEventDto updateCalendarEventDto)
    {
        var existing = await calendarRepository.GetByIdAsync(id);

        if (existing == null)
            throw new NotFoundException($"Event with Id {id} not found");

        await categoryRepository.GetApplicableCategoryAsync(updateCalendarEventDto.CategoryId, AppModule.Calendar);

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

    public Task<byte[]> GenerateIcsFileAsync(GetIcalDto calendarEvent)
    {
        var calendar = new Calendar { Method = "PUBLISH" };

        var iCalEvent = new IcalEvent
        {
            Summary = calendarEvent.Title,
            Description = calendarEvent.Note ?? string.Empty,
            Uid = Guid.NewGuid().ToString()
        };

        if (calendarEvent.IsAllDay)
        {
            // Ical.Net derives CalendarEvent.IsAllDay from Start.HasTime. A date-only
            // CalDateTime serializes as VALUE=DATE, which is what clients need to
            // render the event as all-day instead of a timed UTC event.
            //
            // Use the wall-clock date from the DateTimeOffset, not its UTC date:
            // the client sends local midnight, and converting that to UTC would
            // shift the event to the previous day for positive offsets.
            var startDate = DateOnly.FromDateTime(calendarEvent.StartDateTime.DateTime);

            // FullCalendar sends an exclusive end; without one the event lasts one day.
            var endExclusive = calendarEvent.EndDateTime is { } e
                ? DateOnly.FromDateTime(e.DateTime)
                : startDate.AddDays(1);

            if (endExclusive <= startDate)
                endExclusive = startDate.AddDays(1);

            iCalEvent.Start = new CalDateTime(startDate.Year, startDate.Month, startDate.Day);
            iCalEvent.End = new CalDateTime(endExclusive.Year, endExclusive.Month, endExclusive.Day);
        }
        else
        {
            var start = calendarEvent.StartDateTime.UtcDateTime;
            var end = (calendarEvent.EndDateTime ?? calendarEvent.StartDateTime).UtcDateTime;
            if (end <= start) end = end.AddHours(1);

            iCalEvent.Start = new CalDateTime(start, CalDateTime.UtcTzId);
            iCalEvent.End = new CalDateTime(end, CalDateTime.UtcTzId);
        }

        calendar.Events.Add(iCalEvent);

        var icsContent = new CalendarSerializer().SerializeToString(calendar) ?? string.Empty;
        return Task.FromResult(Encoding.UTF8.GetBytes(icsContent));
    }

    public async Task<int> DeletePastEventsAsync(int months)
    {
        var threshold = DateTime.Now.AddMonths(-months);

        var events = await calendarRepository.GetEventsOlderThanAsync(threshold);
        var deletedCount = 0;

        foreach (var e in events)
        {
            if (string.IsNullOrWhiteSpace(e.RecurrenceRule))
            {
                var dateToCheck = e.EndDateTime ?? e.StartDateTime;

                if (dateToCheck < threshold)
                {
                    await calendarRepository.DeleteEventAsync(e.Id);
                    deletedCount++;
                }

                continue;
            }

            if (e.RecurrenceEnd == null)
                continue;

            if (e.RecurrenceEnd < threshold)
            {
                await calendarRepository.DeleteEventAsync(e.Id);
                deletedCount++;
            }
        }

        return deletedCount;
    }
}