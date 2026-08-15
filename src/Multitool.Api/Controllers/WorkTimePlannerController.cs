using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.WorkTimePlanner;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkTimePlannerController(IWorkTimePlannerService service) : ControllerBase
{
    /// <summary>
    /// Returns work days within a specified date range.
    /// </summary>
    [Authorize]
    [HttpGet("workdays")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWorkDays([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var workDays = await service.GetWorkDaysAsync(startDate, endDate);
        return Ok(workDays);
    }

    /// <summary>
    /// Creates a new work day.
    /// </summary>
    [Authorize]
    [HttpPost("workdays")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateWorkDay([FromBody] CreateWorkDayDto dto)
    {
        var created = await service.CreateWorkDayAsync(dto);
        return CreatedAtAction(nameof(GetWorkDays), new { startDate = created.Date, endDate = created.Date }, created);
    }

    /// <summary>
    /// Updates an existing work day.
    /// </summary>
    [Authorize]
    [HttpPut("workdays/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateWorkDay([FromRoute] int id, [FromBody] UpdateWorkDayDto dto)
    {
        await service.UpdateWorkDayAsync(id, dto);
        return NoContent();
    }

    /// <summary>
    /// Deletes a work day.
    /// </summary>
    [Authorize]
    [HttpDelete("workdays/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteWorkDay([FromRoute] int id)
    {
        await service.DeleteWorkDayAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Returns the week summary for a given year and week number.
    /// </summary>
    [Authorize]
    [HttpGet("weeksummary")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWeekSummary([FromQuery] int year, [FromQuery] int weekNumber)
    {
        var summary = await service.GetWeekSummaryAsync(year, weekNumber);
        return Ok(summary);
    }

    /// <summary>
    /// Calculates and saves the week summary for a given year and week number.
    /// </summary>
    [Authorize]
    [HttpPost("weeksummary")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveWeekSummary([FromQuery] int year, [FromQuery] int weekNumber)
    {
        var summary = await service.SaveWeekSummaryAsync(year, weekNumber);
        return Ok(summary);
    }

    /// <summary>
    /// Returns the current work time settings.
    /// </summary>
    [Authorize]
    [HttpGet("settings")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await service.GetSettingsAsync();
        return Ok(settings);
    }

    /// <summary>
    /// Updates the work time settings.
    /// </summary>
    [Authorize]
    [HttpPut("settings")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateWorkTimeSettingsDto dto)
    {
        await service.UpdateSettingsAsync(dto);
        return NoContent();
    }

    /// <summary>
    /// Returns the number of home office days for a given month.
    /// </summary>
    [Authorize]
    [HttpGet("homeoffice")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHomeOfficeDays([FromQuery] int year, [FromQuery] int month)
    {
        var count = await service.GetHomeOfficeDaysCountAsync(year, month);
        return Ok(new { year, month, homeOfficeDays = count });
    }
}
