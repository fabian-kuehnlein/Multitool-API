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
    public async Task<List<CalendarEventModel>> GetEventsByRangeAsync(DateTime start, DateTime end, string categories)
    {
        var events = await calendarRepository.GetEventsByRangeAsync(start, end, categories);

        var eventModels = events.Adapt<List<CalendarEventModel>>();

        var todos = await todoRepository.GetTodosWithDueDateInRangeAsync(start, end);

        var todoEvents = todos.Select(t => new CalendarEventModel
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

        eventModels.AddRange(todoEvents);

        return eventModels;
    }

    public async Task<List<EventSearchResponseModel>> SearchCalendarEventsAsync(string searchString)
    {
        var result = await calendarRepository.SearchCalendarEventsAsync(searchString);
        return result.Adapt<List<EventSearchResponseModel>>();
    }

    public async Task<long> CreateEventAsync(CreateCalendarEventRequest request)
    {
        await categoryRepository.GetApplicableCategoryAsync(request.CategoryId, AppModule.Calendar);

        return await calendarRepository.CreateEventAsync(request.Adapt<CalendarEvent>());
    }

    public async Task UpdateEventAsync(int id, UpdateCalendarEventRequest request)
    {
        var existing = await calendarRepository.GetByIdAsync(id);

        if (existing == null)
            throw new NotFoundException($"Event with Id {id} not found");

        await categoryRepository.GetApplicableCategoryAsync(request.CategoryId, AppModule.Calendar);

        existing.Title = request.Title;
        existing.Note = request.Note;
        existing.StartDateTime = request.StartDateTime;
        existing.EndDateTime = request.EndDateTime;
        existing.IsAllDay = request.IsAllDay;
        existing.CategoryId = request.CategoryId;
        existing.RecurrenceRule = request.RecurrenceRule;
        existing.RecurrenceEnd = request.RecurrenceEnd;

        await calendarRepository.UpdateEventAsync(existing);
    }

    public async Task DeleteEventAsync(int id)
    {
        var exists = await calendarRepository.GetByIdAsync(id);

        if (exists == null)
            throw new NotFoundException($"Event with Id {id} not found");

        await calendarRepository.DeleteEventAsync(exists);
    }

    public async Task<List<HolidayModel>> GetHolidaysAsync(string year)
    {
        var holidays = await calendarApiClient.GetHolidaysAsync(year);

        if (holidays.Count == 0)
            throw new NotFoundException($"No holidays found for year {year}");

        return holidays.Adapt<List<HolidayModel>>();
    }

    public Task<byte[]> GenerateIcsFileAsync(GetIcalRequest request)
    {
        var calendar = new Calendar { Method = "PUBLISH" };

        var iCalEvent = new IcalEvent
        {
            Summary = request.Title,
            Description = request.Note ?? string.Empty,
            Uid = Guid.NewGuid().ToString()
        };

        if (request.IsAllDay)
        {
            // Ical.Net derives CalendarEvent.IsAllDay from Start.HasTime. A date-only
            // CalDateTime serializes as VALUE=DATE, which is what clients need to
            // render the event as all-day instead of a timed UTC event.
            //
            // Use the wall-clock date from the DateTimeOffset, not its UTC date:
            // the client sends local midnight, and converting that to UTC would
            // shift the event to the previous day for positive offsets.
            var startDate = DateOnly.FromDateTime(request.StartDateTime.DateTime);

            // FullCalendar sends an exclusive end; without one the event lasts one day.
            var endExclusive = request.EndDateTime is { } e
                ? DateOnly.FromDateTime(e.DateTime)
                : startDate.AddDays(1);

            if (endExclusive <= startDate)
                endExclusive = startDate.AddDays(1);

            iCalEvent.Start = new CalDateTime(startDate.Year, startDate.Month, startDate.Day);
            iCalEvent.End = new CalDateTime(endExclusive.Year, endExclusive.Month, endExclusive.Day);
        }
        else
        {
            var start = request.StartDateTime.UtcDateTime;
            var end = (request.EndDateTime ?? request.StartDateTime).UtcDateTime;
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
                    await calendarRepository.DeleteEventAsync(e);
                    deletedCount++;
                }

                continue;
            }

            if (e.RecurrenceEnd == null)
                continue;

            if (e.RecurrenceEnd < threshold)
            {
                await calendarRepository.DeleteEventAsync(e);
                deletedCount++;
            }
        }

        return deletedCount;
    }
}
