namespace MultitoolApi.Businesslogic.Models;

public class EventSearchResponse
{
    public int EventId { get; set; }
    public required string EventTitle { get; set; }
    public string? EventNote { get; set; }
    public required DateTime StartDateTime { get; set; }
}