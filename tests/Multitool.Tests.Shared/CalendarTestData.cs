using Multitool.Domain.Entities.Calendar;
using Multitool.Application.Models.Calendar;

namespace Multitool.Tests.Shared;

public static class CalendarTestData
{
    public static CalendarEvent DefaultEvent => new()
    {
        Id = 1,
        Title = "Team Meeting",
        Note = "Besprechung Projekt Updates",
        StartDateTime = new DateTime(2026, 6, 1, 9, 0, 0),
        EndDateTime = new DateTime(2026, 6, 1, 10, 0, 0),
        IsAllDay = false,
        CategoryId = 1
    };

    public static CalendarEventDto DefaultEventDto => new()
    {
        Id = DefaultEvent.Id.ToString(),
        Title = DefaultEvent.Title,
        StartDateTime = DefaultEvent.StartDateTime,
        EndDateTime = DefaultEvent.EndDateTime,
        IsAllDay = DefaultEvent.IsAllDay,
        CategoryId = DefaultEvent.CategoryId,
        IsTodo = false
    };

    public static CreateCalendarEventDto DefaultCreateEvent => new(
        "Team Meeting",
        "Besprechung Projekt Updates",
        new DateTime(2026, 6, 1, 9, 0, 0),
        new DateTime(2026, 6, 1, 10, 0, 0),
        false,
        1,
        null,
        null
    );

    public static UpdateCalendarEventDto DefaultUpdateEvent => new(
        "Updated Meeting",
        "Aktualisierte Besprechung",
        new DateTime(2026, 6, 1, 10, 0, 0),
        new DateTime(2026, 6, 1, 11, 0, 0),
        false,
        1,
        null,
        null
    );

    public static GetIcalDto DefaultIcalEvent => new(
        "Team Meeting",
        "Besprechung Projekt Updates",
        new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)
    );

    public static Holiday DefaultHoliday => new() { Name = "Neujahr", Date = new DateTime(2026, 1, 1) };
}
