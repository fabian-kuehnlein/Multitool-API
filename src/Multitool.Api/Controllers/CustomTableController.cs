using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.CustomTable;
using Multitool.Domain.Entities.CustomTable;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomTableController(ICustomTableService service) : ControllerBase
{
    /// <summary>
    /// Returns all tables with name and id.
    /// </summary>
    [Authorize]
    [HttpGet("tables")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTableList()
    {
        var tables = await service.GetTableListAsync();
        return Ok(tables);
    }

    /// <summary>
    /// Returns a single table with its columns and rows.
    /// </summary>
    [Authorize]
    [HttpGet("tables/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTable([FromRoute] long id)
    {
        var table = await service.GetTableAsync(id);
        return Ok(table);
    }

    /// <summary>
    /// Creates a new table with an initial column.
    /// </summary>
    [Authorize]
    [HttpPost("tables")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTable([FromBody] CreateTableDto dto)
    {
        var id = await service.CreateTableAsync(dto);
        return Ok(id);
    }

    /// <summary>
    /// Renames a table.
    /// </summary>
    [Authorize]
    [HttpPut("tables/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTable([FromRoute] long id, [FromBody] UpdateTableDto dto)
    {
        await service.UpdateTableAsync(id, dto);
        return NoContent();
    }

    /// <summary>
    /// Deletes a table including all its columns and rows.
    /// </summary>
    [Authorize]
    [HttpDelete("tables/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTable([FromRoute] long id)
    {
        await service.DeleteTableAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Adds a new default column to a table.
    /// </summary>
    [Authorize]
    [HttpPost("tables/{tableId}/columns")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateColumn([FromRoute] long tableId)
    {
        await service.CreateColumnAsync(tableId);
        return NoContent();
    }

    /// <summary>
    /// Updates name, type and order of a column.
    /// </summary>
    [Authorize]
    [HttpPut("columns/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateColumn([FromRoute] long id, [FromBody] UpdateColumnDto dto)
    {
        await service.UpdateColumnAsync(id, dto);
        return NoContent();
    }

    /// <summary>
    /// Updates the order of multiple columns at once.
    /// </summary>
    [Authorize]
    [HttpPut("columns/order")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateColumnOrder([FromBody] List<UpdateColumnOrderDto> columns)
    {
        await service.UpdateColumnOrderAsync(columns);
        return NoContent();
    }

    /// <summary>
    /// Deletes a column and all its cell data.
    /// </summary>
    [Authorize]
    [HttpDelete("tables/{tableId}/columns/{columnId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteColumn([FromRoute] long tableId, [FromRoute] long columnId)
    {
        await service.DeleteColumnAsync(tableId, columnId);
        return NoContent();
    }

    /// <summary>
    /// Adds a new empty row to a table.
    /// </summary>
    [Authorize]
    [HttpPost("tables/{tableId}/rows")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateRow([FromRoute] long tableId)
    {
        await service.CreateRowAsync(tableId);
        return NoContent();
    }

    /// <summary>
    /// Updates the order of multiple rows at once.
    /// </summary>
    [Authorize]
    [HttpPut("rows/order")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRowOrder([FromBody] List<RowOrderUpdateDto> rows)
    {
        await service.UpdateRowOrderAsync(rows);
        return NoContent();
    }

    /// <summary>
    /// Deletes multiple rows by id.
    /// </summary>
    [Authorize]
    [HttpDelete("tables/{tableId}/rows")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRows([FromRoute] long tableId, [FromBody] List<long> rowIds)
    {
        await service.DeleteRowsAsync(tableId, rowIds);
        return NoContent();
    }

    /// <summary>
    /// Creates or updates a single cell value.
    /// </summary>
    [Authorize]
    [HttpPut("rows/{rowId}/cells/{columnId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetCell([FromRoute] long rowId, [FromRoute] long columnId, [FromBody] object? newValue)
    {
        await service.UpsertCellAsync(rowId, columnId, newValue);
        return NoContent();
    }
}
