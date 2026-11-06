namespace Multitool.Application.Models.Calendar;

public record GetIcalDto(
    string Title,
    string? Note,
    DateTimeOffset StartDateTime,
    DateTimeOffset? EndDateTime,
    bool IsAllDay
);
