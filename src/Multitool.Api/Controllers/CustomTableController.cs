using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MultitoolApi.WebApi.Models.CustomTable;
using Multitool.Application.Interfaces;
using Multitool.Domain.Enums;

namespace Multitool.Api.Controllers;

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
    public async Task<IActionResult> CreateTable([FromBody] CreateTableDto dto)
    {
        var id = await _service.CreateTableAsync(dto);
        return Ok(id);
    }

    [HttpPut("UpdateTable")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateTable([FromQuery] long tableId, [FromQuery] string newName)
    {
        await _service.UpdateTableAsync(tableId, newName);
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
    /// following Methodes are for handeling creating, updating and deleting columns
    /// </summary>

    [HttpPost("CreateColumn")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateColumn([FromQuery] long tableId)
    {
        await _service.CreateColumnAsync(tableId);
        return Ok();
    }

    [HttpPut("UpdateColumn")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateColumn([FromQuery] long columnId, [FromBody] UpdateColumnDto dto)
    {
        await _service.UpdateColumnAsync(columnId, dto);
        return Ok();
    }

    [HttpPut("UpdateColumnOrder")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateColumnOrder([FromBody] List<UpdateColumnOrderDto> columns)
    {
        await _service.UpdateColumnOrderAsync(columns);
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
    /// Following Methodes are for handeling creating and deleting rows
    /// </summary>

    [HttpPost("CreateRow")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateRow([FromQuery] long tableId)
    {
        await _service.CreateRowAsync(tableId);
        return Ok();
    }

    [HttpPut("UpdateRowOrder")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateRowOrder([FromBody] List<RowOrderUpdateDto> list)
    {
        await _service.UpdateRowOrderAsync(list);
        return Ok();
    }

    [HttpDelete("DeleteRows")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteRows([FromQuery] long tableId, [FromBody] List<long> rows)
    {
        await _service.DeleteRowsAsync(tableId, rows);
        return Ok();
    }

    /// <summary>
    /// Updates singular cell, to not send the whole row when editing single cells
    /// </summary>
    [HttpPut("SetCell")]
    [Produces("application/json")]
    public async Task<IActionResult> SetCell([FromQuery] long rowId, [FromQuery] long columnId, [FromBody] object? newValue)
    {
        await _service.UpsertCellAsync(rowId, columnId, newValue);
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
            new ColumnInfo(5, "Aktiv", CustomDataType.Bool, 4),
            new ColumnInfo(6, "Datum", CustomDataType.Date, 5)
        };

        var rows = new List<RowInfo>
        {
            new RowInfo(1,  new() { {1,"Max"},   {2,"Mustermann"}, {3,25}, {4,"Musterstr. 3"},   {5,true},  {6, DateTime.UtcNow.AddDays(-10)} }, 0),
            new RowInfo(2,  new() { {1,"Anna"},  {2,"Schmidt"},    {3,20}, {4,"Berliner Str. 2"}, {5,false}, {6, DateTime.UtcNow.AddDays(-9)}  }, 1),
            new RowInfo(3,  new() { {1,"Tina"},  {2,"Meyer"},      {3,31}, {4,"Hauptweg 12"},    {5,true},  {6, DateTime.UtcNow.AddDays(-8)}  }, 2),
            new RowInfo(4,  new() { {1,"Lars"},  {2,"König"},      {3,28}, {4,"Ring 7"},         {5,false}, {6, DateTime.UtcNow.AddDays(-7)}  }, 3),
            new RowInfo(5,  new() { {1,"Paul"},  {2,"Fischer"},    {3,22}, {4,"Am See 5"},       {5,true},  {6, DateTime.UtcNow.AddDays(-6)}  }, 4),
            new RowInfo(6,  new() { {1,"Eva"},   {2,"Schulz"},     {3,34}, {4,"Gartenweg 9"},    {5,true},  {6, DateTime.UtcNow.AddDays(-5)}  }, 5),
            new RowInfo(7,  new() { {1,"Omar"},  {2,"Ali"},        {3,27}, {4,"Markt 1"},        {5,false}, {6, DateTime.UtcNow.AddDays(-4)}  }, 6),
            new RowInfo(8,  new() { {1,"Sara"},  {2,"Bauer"},      {3,23}, {4,"Wiesenstr. 4"},   {5,true},  {6, DateTime.UtcNow.AddDays(-3)}  }, 7),
            new RowInfo(9,  new() { {1,"Jonas"}, {2,"Krüger"},     {3,30}, {4,"Dorfplatz 8"},    {5,false}, {6, DateTime.UtcNow.AddDays(-2)}  }, 8),
            new RowInfo(10, new() { {1,"Mia"},   {2,"Hoffmann"},   {3,26}, {4,"Allee 6"},        {5,true},  {6, DateTime.UtcNow.AddDays(-1)}  }, 9)
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
