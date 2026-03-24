using System.ComponentModel.DataAnnotations;

namespace Multitool.Domain.Entities.Calendar;

public class CreateCalendarEvent
{
    public required string EventTitle { get; set; }
    public string? EventNote { get; set; }
    public required DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public required bool IsAllDay { get; set; }
    public required int CategoryId { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateTime? RecurrenceEnd { get; set; }
}
