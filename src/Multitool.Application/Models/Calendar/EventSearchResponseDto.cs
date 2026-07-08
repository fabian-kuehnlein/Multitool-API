namespace Multitool.Application.Models.Calendar;

public record EventSearchResponseDto(
    int EventId,
    string EventTitle,
    string? EventNote,
    DateTime StartDateTime,
    string? RecurrenceRule,
    DateTime? RecurrenceEnd
);
