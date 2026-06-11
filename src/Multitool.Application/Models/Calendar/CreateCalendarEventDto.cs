namespace Multitool.Application.Models.Calendar;

public record CreateCalendarEventDto(
    string Title,
    string? Note,
    DateTime StartDateTime,
    DateTime? EndDateTime,
    bool IsAllDay,
    int CategoryId,
    string? RecurrenceRule,
    DateTime? RecurrenceEnd
);
