namespace Multitool.Application.Models.Calendar;

public record GetIcalDto(
    string Title,
    string? Note,
    DateTime StartDateTime,
    DateTime? EndDateTime
);
