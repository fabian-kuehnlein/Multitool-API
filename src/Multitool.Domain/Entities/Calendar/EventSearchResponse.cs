namespace Multitool.Domain.Entities.Calendar;

public class EventSearchResponse
{
    public int EventId { get; set; }
    public required string EventTitle { get; set; }
    public string? EventNote { get; set; }
    public required DateTime StartDateTime { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateTime? RecurrenceEnd { get; set; }
}