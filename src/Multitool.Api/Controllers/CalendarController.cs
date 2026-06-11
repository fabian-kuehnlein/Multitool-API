using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Calendar;
using Multitool.Application.Models.Calendar;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalendarController(ICalendarService calendarService) : ControllerBase
{
    /// <summary>
    /// Gets calendar events within a specified date range.
    /// </summary>
    [Authorize]
    [HttpGet("events")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEventsByRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string? categories)
    {
        var events = await calendarService.GetEventsByRangeAsync(startDate, endDate, categories ?? string.Empty);
        return Ok(events);
    }

    /// <summary>
    /// Searches calendar events based on a search string that matches the title or note of the event.
    /// </summary>
    [Authorize]
    [HttpGet("events/search")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchEvents([FromQuery][Required] string searchString)
    {
        var events = await calendarService.SearchCalendarEventsAsync(searchString);
        return Ok(events);
    }

    /// <summary>
    /// Inserts a new calendar event
    /// </summary>
    [Authorize]
    [HttpPost("events")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InsertEvent([FromBody] CreateCalendarEventDto calendarEvent)
    {
        var id = await calendarService.InsertEventAsync(calendarEvent);
        return Ok(id);
    }

    /// <summary>
    /// Updates an existing calendar event
    /// </summary>
    [Authorize]
    [HttpPut("events")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEvent([FromBody] CalendarEvent calendarEvent)
    {
        await calendarService.UpdateEventAsync(calendarEvent);
        return NoContent();
    }

    /// <summary>
    /// Deletes an existing calendar event via its Id
    /// </summary>
    [Authorize]
    [HttpDelete("events/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteEvent([FromRoute] int id)
    {
        await calendarService.DeleteEventAsync(id);
        return NoContent();
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
    public async Task<IActionResult> GetHolidays([FromRoute]string year)
    {
        var result = await calendarService.GetHolidaysAsync(year);
        return Ok(result);
    }
}
