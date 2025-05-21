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
}
