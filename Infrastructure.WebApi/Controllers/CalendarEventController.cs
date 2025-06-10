using AutoMapper;
using MultitoolApi.Businesslogic.Models;
using MultitoolApi.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CalendarEventController : ControllerBase
{
    private readonly ICalendarService _service;
    private readonly IMapper _mapper;
    private readonly ILogger<CalendarEventController> _logger;

    public CalendarEventController(ICalendarService service, IMapper mapper, ILogger<CalendarEventController> logger)
    {
        _service = service;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet("GetEventsByRange")]
    [Produces("application/json")]
    public async Task<IActionResult> GetEventsByRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string? categories)
    {
        var events = await _service.GetEventsByRangeAsync(startDate, endDate, categories ?? string.Empty);
        return Ok(events);
    }

    [HttpGet("SearchEvents")]
    [Produces("application/json")]
    public async Task<IActionResult> SearchEvents([FromQuery] string searchString)
    {
        var events = await _service.SearchCalendarEventsAsync(searchString);
        return Ok(_mapper.Map<List<EventSearchResponseDTO>>(events));
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
        return Ok();
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
