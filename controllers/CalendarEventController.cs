using CalendarApp.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CalendarEventController : ControllerBase
{
    private readonly CalendarEventRepository _repository;

    public CalendarEventController(CalendarEventRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("GetAllEvents")]
    [Produces("application/json")]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _repository.GetAllEventsAsync();
        return Ok(events);
    }

    [HttpGet("GetEventsByRange")]
    [Produces("application/json")]
    public async Task<IActionResult> GetEventsByRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var events = await _repository.GetEventsByRangeAsync(startDate, endDate);
        return Ok(events);
    }

    [HttpPost("InsertEvent")]
    [Produces("application/json")]
    public async Task<IActionResult> InsertEvent([FromBody] CalendarEvent calendarEvent)
    {
        if (calendarEvent == null)
        {
            return BadRequest("Invalid event data.");
        }

        await _repository.InsertEventAsync(calendarEvent);
        return CreatedAtAction(nameof(GetAllEvents), new { id = calendarEvent.EventId }, calendarEvent);
    }
}
