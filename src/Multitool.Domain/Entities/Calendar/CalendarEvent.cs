namespace Multitool.Domain.Entities.Calendar;

public class CalendarEvent
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Note { get; set; }
    public required DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public required bool IsAllDay { get; set; }
    public required int CategoryId { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateTime? RecurrenceEnd { get; set; }
}
