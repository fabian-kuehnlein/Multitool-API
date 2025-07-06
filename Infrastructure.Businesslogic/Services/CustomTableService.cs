using AutoMapper;
using MultitoolApi.Businesslogic.Models;
using MultitoolApi.DataAccessLayer.Models;
using MultitoolApi.WebApi.Models.CustomTable;

namespace MultitoolApi.Infrastructure.Businesslogic.Services;

public class CustomTableService : ICustomTableService
{
    private readonly ICustomTableRepository _repository;
    private readonly IMapper _mapper;

    public CustomTableService(ICustomTableRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<TableOverview>> GetTableListAsync()
    {
        return await _repository.GetTableListAsync();
    }

    public async Task<TableDetail?> GetTableAsync(long tableId)
    {
        return await _repository.GetTableAsync(tableId);
    }

    public async Task CreateTableAsync(string name)
    {
        await _repository.CreateTableAsync(name);
    }

    public async Task UpdateTableAsync(long tableId, string name)
    {
        await _repository.UpdateTableAsync(tableId, name);
    }

    public async Task DeleteTableAsync(long tableId)
    {
        await _repository.DeleteTableAsync(tableId);
    }

    /* ---------- Spalten ---------- */

    public async Task<List<ColumnInfo>> GetColumnsAsync(long tableId)
    {
        return await _repository.GetColumnsAsync(tableId);
    }

    public async Task CreateColumnAsync(long tableId, CreateColumnDto dto)
    {
        await _repository.CreateColumnAsync(tableId, dto);
    }

    public async Task UpdateColumnAsync(long tableId, long columnId, UpdateColumnDto dto)
    {
        await _repository.UpdateColumnAsync(tableId, columnId, dto);
    }

    public async Task DeleteColumnAsync(long tableId, long columnId)
    {
        await _repository.DeleteColumnAsync(tableId, columnId);
    }

    /* ---------- Rows ---------- */

    public async Task<List<RowInfo>> GetRowsAsync(long tableId, int pageNr, int pageSize)
    {
        return await _repository.GetRowsAsync(tableId, pageNr, pageSize);
    }

    public async Task CreateRowAsync(long tableId, Dictionary<long, object?> cells)
    {
        await _repository.CreateRowAsync(tableId, cells);
    }

    public async Task UpdateRowAsync(long tableId, long rowId, Dictionary<long, object?> cells)
    {
        await _repository.UpdateRowAsync(tableId, rowId, cells);
    }

    public async Task DeleteRowAsync(long tableId, long rowId)
    {
        await _repository.DeleteRowAsync(tableId, rowId);
    }

    public async Task UpdateCellAsync(long rowId, long columnId, object? newValue)
    {
        await _repository.UpdateCellAsync(rowId, columnId, newValue);
    }
}