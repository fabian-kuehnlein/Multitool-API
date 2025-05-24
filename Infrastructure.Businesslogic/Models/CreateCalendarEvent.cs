using System.ComponentModel.DataAnnotations;

namespace CalendarApi.Businesslogic.Models;

public class CreateCalendarEvent
{
    [Required]
    public string EventTitle { get; set; } 
    public string? EventNote { get; set; }
    [Required]
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    [Required]
    public bool IsAllDay { get; set; }
    [Required]
    public int CategoryId { get; set; }
}
