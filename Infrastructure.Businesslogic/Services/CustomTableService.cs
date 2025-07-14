using AutoMapper;
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

    public async Task<long> CreateTableAsync(CreateTableDto dto)
    {
        return await _repository.CreateTableAsync(dto);
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

    public async Task CreateColumnAsync(long tableId)
    {
        await _repository.CreateColumnAsync(tableId);
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

    public async Task CreateRowAsync(long tableId)
    {
        await _repository.CreateRowAsync(tableId);
    }

    public async Task UpdateRowOrderAsync(List<RowOrderUpdateDto> list)
    {
        await _repository.UpdateRowOrderAsync(list);
    }

    public async Task DeleteRowsAsync(long tableId, List<long> rows)
    {
        await _repository.DeleteRowsAsync(tableId, rows);
    }

    public async Task UpsertCellAsync(long rowId, long columnId, object? newValue)
    {
        await _repository.UpsertCellAsync(rowId, columnId, newValue);
    }
}