using Microsoft.AspNetCore.Mvc;
using MultitoolApi.WebApi.Models.CustomTable;
using Multitool.Application.Interfaces;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomTableController(ICustomTableService service) : ControllerBase
{
    /// <summary>
    /// Returns all tables with name and id.
    /// </summary>
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
    [HttpGet("tables/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTable(long id)
    {
        try
        {
            var table = await service.GetTableAsync(id);

            if (table is null)
                throw new KeyNotFoundException($"Table with id {id} not found.");

            return Ok(table);
        }
        catch (Exception ex)
        {
            
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Creates a new table with an initial column.
    /// </summary>
    [HttpPost("tables")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTable([FromBody] CreateTableDto dto)
    {
        try
        {
            var id = await service.CreateTableAsync(dto);
            return Ok(id);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Renames a table.
    /// </summary>
    [HttpPut("tables/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTable(long id, [FromBody] string newName)
    {
        try
        {
            await service.UpdateTableAsync(id, newName);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Deletes a table including all its columns and rows.
    /// </summary>
    [HttpDelete("tables/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTable(long id)
    {
        try
        {
            await service.DeleteTableAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Adds a new default column to a table.
    /// </summary>
    [HttpPost("tables/{tableId}/columns")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateColumn(long tableId)
    {
        try
        {
            await service.CreateColumnAsync(tableId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Updates name, type and order of a column.
    /// </summary>
    [HttpPut("columns/{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateColumn(long id, [FromBody] UpdateColumnDto dto)
    {
        try
        {
            await service.UpdateColumnAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Updates the order of multiple columns at once.
    /// </summary>
    [HttpPut("columns/order")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateColumnOrder([FromBody] List<UpdateColumnOrderDto> columns)
    {
        try
        {
            await service.UpdateColumnOrderAsync(columns);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Deletes a column and all its cell data.
    /// </summary>
    [HttpDelete("tables/{tableId}/columns/{columnId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteColumn(long tableId, long columnId)
    {
        try
        {
            await service.DeleteColumnAsync(tableId, columnId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Adds a new empty row to a table.
    /// </summary>
    [HttpPost("tables/{tableId}/rows")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateRow(long tableId)
    {
        try
        {
            await service.CreateRowAsync(tableId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Updates the order of multiple rows at once.
    /// </summary>
    [HttpPut("rows/order")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRowOrder([FromBody] List<RowOrderUpdateDto> rows)
    {
        try
        {
            await service.UpdateRowOrderAsync(rows);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Deletes multiple rows by id.
    /// </summary>
    [HttpDelete("tables/{tableId:long}/rows")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRows(long tableId, [FromBody] List<long> rowIds)
    {
        try
        {
            await service.DeleteRowsAsync(tableId, rowIds);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Creates or updates a single cell value.
    /// </summary>
    [HttpPut("rows/{rowId}/cells/{columnId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetCell(long rowId, long columnId, [FromBody] object? newValue)
    {
        try
        {
            await service.UpsertCellAsync(rowId, columnId, newValue);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}