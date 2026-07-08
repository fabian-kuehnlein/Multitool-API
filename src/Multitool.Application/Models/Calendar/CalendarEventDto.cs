namespace Multitool.Application.Models.Calendar;

public record CalendarEventDto {
    public string Id { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Note { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public bool IsAllDay { get; set; }
    public int CategoryId { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateTime? RecurrenceEnd { get; set; }
    public bool IsTodo { get; set; }
}