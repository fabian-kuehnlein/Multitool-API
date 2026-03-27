using System.ComponentModel.DataAnnotations;
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEventsByRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string? categories)
    {
        var events = await service.GetEventsByRangeAsync(startDate, endDate, categories ?? string.Empty);
        return Ok(events);
    }

    /// <summary>
    /// Searches calendar events based on a search string that matches the title or note of the event.
    /// </summary>
    [HttpGet("events/search")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchEvents([FromQuery][Required] string searchString)
    {
        var events = await service.SearchCalendarEventsAsync(searchString);
        return Ok(events);
    }

    /// <summary>
    /// Inserts a new calendar event
    /// </summary>
    [HttpPost("events")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InsertEvent([FromBody] CreateCalendarEvent calendarEvent)
    {
        var id = await service.InsertEventAsync(calendarEvent);
        return Ok(id);
    }

    /// <summary>
    /// Updates an existing calendar event
    /// </summary>
    [HttpPut("events/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEvent([FromBody] CalendarEvent calendarEvent)
    {
        await service.UpdateEventAsync(calendarEvent);
        return NoContent();
    }

    /// <summary>
    /// Deletes an existing calendar event via its Id
    /// </summary>
    [HttpDelete("events/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteEvent([FromRoute] int id)
    {
        await service.DeleteEventAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Returns a list of all available categories for calendar events
    /// </summary>
    [HttpGet("categories")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCategories()
    {     
        var categories = await service.GetCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Returns a list of all holidays for a given year.
    /// Request to open source API: https://get.api-feiertage.de
    /// </summary>
    [HttpGet("holidays/{year}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHolidays([FromRoute]string year)
    {
        var result = await service.GetHolidaysAsync(year);
        return Ok(result);
    }
}
