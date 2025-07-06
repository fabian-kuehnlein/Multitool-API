using AutoMapper;
using MultitoolApi.Businesslogic.Models;
using MultitoolApi.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using MultitoolApi.Infrastructure.Businesslogic.Services;
using MultitoolApi.WebApi.Models.CustomTable;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MultitoolApi.Infrastructure.DataAccessLayer.Models.CustomTable;

[ApiController]
[Route("api/[controller]")]
public class CustomTableController : ControllerBase
{
    private readonly ICustomTableService _service;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomTableController> _logger;

    public CustomTableController(ICustomTableService service, IMapper mapper, ILogger<CustomTableController> logger)
    {
        _service = service;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the Tables that are shown in Frontend to select and open them
    /// </summary>
    /// <returns> List of Tables with Name and Id</returns>
    [HttpGet("GetTableList")]
    [Produces("application/json")]
    public async Task<IActionResult> GetTableList()
    {
        var tables = await _service.GetTableListAsync();
        return Ok(tables);
    }

    /// <summary>
    /// Following routes are for handeling Table reading, creating, updating and deleting
    /// </summary>
    /// 
    [HttpGet("GetTable")]
    [Produces("application/json")]
    public async Task<IActionResult> GetTableAsync([FromQuery] long tableId)
    {
        var table = await _service.GetTableAsync(tableId);
        return table is null ? NotFound() : Ok(table);
    }

    [HttpPost("CreateTable")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateTable([FromQuery] string name)
    {
        await _service.CreateTableAsync(name);
        return Ok();
    }

    [HttpPut("UpdateTable")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateTable([FromQuery] long tableId, [FromQuery] string name)
    {
        await _service.UpdateTableAsync(tableId, name);
        return Ok();
    }

    [HttpDelete("DeleteTable")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteTable([FromQuery] long tableId)
    {
        await _service.DeleteTableAsync(tableId);
        return Ok();
    }

    /// <summary>
    /// following Methodes are for handeling reading and creating columns
    /// </summary>
    [HttpGet("GetColumns")]
    [Produces("application/json")]
    public async Task<IActionResult> GetColumns([FromQuery] long tableId)
    {
        var columns = await _service.GetColumnsAsync(tableId);
        return Ok(columns);
    }

    [HttpPost("CreateColumn")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateColumn([FromQuery] long tableId, [FromBody] CreateColumnDto dto)
    {
        await _service.CreateColumnAsync(tableId, dto);
        return Ok();
    }

    [HttpPut("UpdateColumn")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateColumn([FromQuery] long tableId, [FromQuery] long columnId, [FromBody] UpdateColumnDto dto)
    {
        await _service.UpdateColumnAsync(tableId, columnId, dto);
        return Ok();
    }

    [HttpDelete("DeleteColumn")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteColumn([FromQuery] long tableId, [FromQuery] long columnId)
    {
        await _service.DeleteColumnAsync(tableId, columnId);
        return Ok();
    }

    /// <summary>
    /// Following Methodes are for handeling reading, creating, updating and deleting rows
    /// </summary>
    [HttpGet("GetRows")]
    [Produces("application/json")]
    public async Task<IActionResult> GetRows([FromQuery] long tableId, [FromQuery] int pageNr = 1, int pageSize = 10)
    {
        pageNr = pageNr < 1 ? 1 : pageNr;
        var rows = await _service.GetRowsAsync(tableId, pageNr, pageSize);
        return Ok(rows);
    }

    [HttpPost("CreateRow")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateRow([FromQuery] long tableId, [FromBody] CreateRowDto dto)
    {
        await _service.CreateRowAsync(tableId, dto.Cells);
        return Ok();
    }

    [HttpPut("UpdateRow")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateRow([FromQuery] long tableId, [FromQuery] long rowId, [FromBody] UpdateRowDto dto)
    {
        await _service.UpdateRowAsync(tableId, rowId, dto.Cells);
        return Ok();
    }

    [HttpDelete("DeleteRow")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteRow([FromQuery] long tableId, [FromQuery] long rowId)
    {
        await _service.DeleteRowAsync(tableId, rowId);
        return Ok();
    }

    /// <summary>
    /// Updates singular cell, to not send the whole row when editing single cells
    /// </summary>
    [HttpPut("UpdateCell")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateCell([FromQuery] long rowId, [FromQuery] long columnId, [FromBody] object? newValue)
    {
        await _service.UpdateCellAsync(rowId, columnId, newValue);
        return Ok();
    }

    [HttpGet("GetDevTable")]
    [Produces("application/json")]
    public ActionResult<TableDetail> GetDevTable()
    {
        var columns = new List<ColumnInfo>
        {
            new ColumnInfo(1, "Name", CustomDataType.String, 0),
            new ColumnInfo(2, "Vorname", CustomDataType.String, 1),
            new ColumnInfo(3, "Alter", CustomDataType.Int, 2),
            new ColumnInfo(4, "Straße", CustomDataType.String, 3),
            new ColumnInfo(5, "Aktiv", CustomDataType.Bool, 4)
        };

        var rows = new List<RowInfo>
        {
            new RowInfo(1, new Dictionary<long, object?>
            {
                { 1, "Max" },
                { 2, "Mustermann" },
                { 3, 25 },
                { 4, "Musterstraße 3" },
                { 5, true },
            }),
            new RowInfo(2, new Dictionary<long, object?>
            {
                { 1, "Anna" },
                { 2, "Schmidt" },
                { 3, 20 },
                { 4, "Berliner Straße 2" },
                { 5, false },
            })
        };

        var table = new TableDetail(
            TableId: 999,
            Name: "TestTabelle",
            CreatedAt: DateTime.UtcNow,
            Columns: columns,
            Rows: rows
        );

        return Ok(table);
    }
}
