namespace Multitool.Application.Models.Calendar;

public record GetIcalRequest(
    string Title,
    string? Note,
    DateTimeOffset StartDateTime,
    DateTimeOffset? EndDateTime,
    bool IsAllDay
);
