using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multitool.Api.Extensions;
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
    /// Creates or updates the work day for the given date (upsert).
    /// </summary>
    [Authorize]
    [HttpPut("workdays")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpsertWorkDay([FromBody] UpsertWorkDayDto dto)
    {
        await service.UpsertWorkDayAsync(User.GetUserId(), dto);
        return NoContent();
    }

    /// <summary>
    /// Locks or unlocks the work day with the given id.
    /// </summary>
    [Authorize]
    [HttpPut("workdays/{id}/lock")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetWorkDayLock([FromRoute] int id, [FromBody] LockWorkDayDto dto)
    {
        await service.SetWorkDayLockedAsync(id, dto.IsLocked);
        return NoContent();
    }

    /// <summary>
    /// Deletes the work day with the given id.
    /// </summary>
    [Authorize]
    [HttpDelete("workdays/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteWorkDay([FromRoute] int id)
    {
        await service.DeleteWorkDayAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Returns the current total overtime balance summed across all work days.
    /// </summary>
    [Authorize]
    [HttpGet("overtime-balance")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTotalOvertime()
    {
        var totalOvertime = await service.GetTotalOvertimeAsync();
        return Ok(new { totalOvertime });
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
