using System.ComponentModel.DataAnnotations;

namespace MultitoolApi.DataAccessLayer.Models;

public class CreateCalendarEventDAO
{
    public required string EventTitle { get; set; } 
    public string? EventNote { get; set; }
    public required DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public required bool IsAllDay { get; set; }
    public required int CategoryId { get; set; }
}
