namespace Multitool.Application.Models.Calendar;

public record EventSearchResponseModel(
    int EventId,
    string EventTitle,
    string? EventNote,
    DateTime StartDateTime,
    string? RecurrenceRule,
    DateTime? RecurrenceEnd
);
