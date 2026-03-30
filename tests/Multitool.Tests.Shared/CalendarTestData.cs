using Multitool.Domain.Entities.Calendar;

namespace Multitool.Api.Tests;

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

    public static readonly CreateCalendarEvent DefaultCreateEvent = new()
    {
        Title = "Team Meeting",
        Note = "Besprechung Projekt Updates",
        StartDateTime = new DateTime(2026, 6, 1, 9, 0, 0),
        EndDateTime = new DateTime(2026, 6, 1, 10, 0, 0),
        IsAllDay = false,
        CategoryId = 1
    };

    public static readonly Category DefaultCategory = new() { Id = 1, Name = "Arbeit", Color = "#FF0000" };

    public static readonly Holiday DefaultHoliday = new() { Name = "Neujahr", Date = new DateTime(2026, 1, 1)};
}