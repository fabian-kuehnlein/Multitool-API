using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Entities.Category;
using Multitool.Application.Models.Calendar;

namespace Multitool.Tests.Shared;

public class CalendarTestData
{
    public static readonly CalendarEvent DefaultEvent = new()
    {
        Id = 1,
        Title = "Team Meeting",
        Note = "Besprechung Projekt Updates",
        StartDateTime = new DateTime(2026, 6, 1, 9, 0, 0),
        EndDateTime = new DateTime(2026, 6, 1, 10, 0, 0),
        IsAllDay = false,
        CategoryId = 1
    };

    public static readonly CreateCalendarEventDto DefaultCreateEvent = new(
        "Team Meeting",
        "Besprechung Projekt Updates",
        new DateTime(2026, 6, 1, 9, 0, 0),
        new DateTime(2026, 6, 1, 10, 0, 0),
        false,
        1,
        null,
        null
    );

    public static readonly Category DefaultCategory = new() { Id = 1, Name = "Arbeit", Color = "#FF0000" };

    public static readonly Holiday DefaultHoliday = new() { Name = "Neujahr", Date = new DateTime(2026, 1, 1)};
}