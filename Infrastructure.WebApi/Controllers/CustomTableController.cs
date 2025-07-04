using AutoMapper;
using MultitoolApi.Businesslogic.Models;
using MultitoolApi.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using MultitoolApi.Infrastructure.Businesslogic.Services;

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

    [HttpGet("GetTableList")]
    [Produces("application/json")]
    public async Task<IActionResult> GetTableList()
    {
        var tables = await _service.GetTableListAsync();
        return Ok(tables);
    }

    [HttpGet("GetTableById")]
    [Produces("application/json")]
    public async Task<IActionResult> GetTableById([FromQuery] long tableId)
    {
        // var table = await _service.GetTableByIdAsync(tableId);
        // if (table == null)
        // {
        //     return NotFound();
        // }
        // return Ok(table);
        return Ok();
    }

    // [HttpPost("CreateTable")]
    // [Produces("application/json")]
    // public async Task<IActionResult> CreateTable([FromBody] CreateTableDto createTableDto)
    // {
    //     return Ok();
    // }
}
