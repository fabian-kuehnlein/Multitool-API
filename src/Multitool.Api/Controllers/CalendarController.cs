using Microsoft.AspNetCore.Mvc;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Calendar;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalendarController(ICalendarService service) : ControllerBase
{
    /// <summary>
    /// Gets calendar events within a specified date range.
    /// </summary>
    [HttpGet("events")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEventsByRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string? categories)
    {
        try
        {
            var events = await service.GetEventsByRangeAsync(startDate, endDate, categories ?? string.Empty);

            if (events is null || events.Count <= 0)
                return NotFound();

            return Ok(events);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Searches calendar events based on a search string that matches the title or note of the event.
    /// </summary>
    [HttpGet("events/search")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchEvents([FromQuery] string searchString)
    {
        try
        {  
            var events = await service.SearchCalendarEventsAsync(searchString);

            if (events is null || events.Count <= 0)
                return NotFound();

            return Ok(events);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Inserts a new calendar event
    /// </summary>
    [HttpPost("events")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InsertEvent([FromBody] CreateCalendarEvent calendarEvent)
    {
        try
        {
            await service.InsertEventAsync(calendarEvent);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing calendar event
    /// </summary>
    [HttpPut("events/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEvent([FromBody] CalendarEvent calendarEvent)
    {
        try
        {
            await service.UpdateEventAsync(calendarEvent);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Deletes an existing calendar event via its Id
    /// </summary>
    [HttpDelete("events/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteEvent([FromQuery] int id)
    {
        try
        {
            await service.DeleteEventAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Returns a list of all available categories for calendar events
    /// </summary>
    [HttpGet("categories")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCategories()
    {
        try
        {       
            var categories = await service.GetCategoriesAsync();

            if (categories is null || categories.Count <= 0)
                return NotFound();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Returns a list of all holidays for a given year.
    /// Request to open source API: https://get.api-feiertage.de
    /// </summary>
    [HttpGet("holidays/{year}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHolidays([FromQuery] string year)
    {
        try
        {
            var result = await service.GetHolidaysAsync(year);

            if (result is null || result.Count <= 0)
                return NotFound();
    
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
