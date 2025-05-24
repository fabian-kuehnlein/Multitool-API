namespace CalendarApi.Webapi.Models;

public class CalendarEventDTO
{
    public int EventId { get; set; }
    public string EventTitle { get; set; }
    public string? EventNote { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public bool IsAllDay { get; set; }
    public int CategoryId { get; set; }
}
