namespace Multitool.Application.Models.Calendar;

public record UpdateCalendarEventRequest(
    string Title,
    string? Note,
    DateTime StartDateTime,
    DateTime? EndDateTime,
    bool IsAllDay,
    int CategoryId,
    string? RecurrenceRule,
    DateTime? RecurrenceEnd
);
