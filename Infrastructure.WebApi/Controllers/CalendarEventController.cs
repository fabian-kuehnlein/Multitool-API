using AutoMapper;
using CalendarApi.Businesslogic.Models;
using CalendarApi.Webapi.Models;
using CalendarApi.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CalendarEventController : ControllerBase
{
    private readonly ICalendarService _service;
    private readonly IMapper _mapper;

    public CalendarEventController(ICalendarService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet("GetEventsByRange")]
    [Produces("application/json")]
    public async Task<IActionResult> GetEventsByRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var events = await _service.GetEventsByRangeAsync(startDate, endDate);
        return Ok(events);
    }

    [HttpPost("InsertEvent")]
    [Produces("application/json")]
    public async Task<IActionResult> InsertEvent([FromBody] CreateCalendarEventDTO calendarEvent)
    {
        await _service.InsertEventAsync(_mapper.Map<CreateCalendarEvent>(calendarEvent));
        return Ok();
    }

    [HttpPut("UpdateEvent")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateEvent([FromBody] CalendarEventDTO calendarEvent)
    {
        await _service.UpdateEventAsync(_mapper.Map<CalendarEvent>(calendarEvent));
        return Ok(calendarEvent);
    }

    [HttpDelete("DeleteEvent")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteEvent([FromQuery] int eventId)
    {
        await _service.DeleteEventAsync(eventId);
        return Ok();
    }

    [HttpGet("GetCategories")]
    [Produces("application/json")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _service.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("GetHolidays")]
    [Produces("application/json")]
    public async Task<IActionResult> GetHolidays([FromQuery] string year)
    {
        var result = await _service.GetHolidaysAsync(year);
        return Ok(result);
    }
}
