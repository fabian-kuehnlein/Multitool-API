using Multitool.Application.Interfaces;
using Multitool.Domain.Interfaces;
using MultitoolApi.WebApi.Models.CustomTable;

namespace Multitool.Application.Services;

public class CustomTableService(ICustomTableRepository repository) : ICustomTableService
{
    public async Task<List<TableOverview>> GetTableListAsync()
    {
        return await repository.GetTableListAsync();
    }

    public async Task<TableDetail?> GetTableAsync(long tableId)
    {
        return await repository.GetTableAsync(tableId);
    }

    public async Task<long> CreateTableAsync(CreateTableDto dto)
    {
        return await repository.CreateTableAsync(dto);
    }

    public async Task UpdateTableAsync(long tableId, string name)
    {
        await repository.UpdateTableAsync(tableId, name);
    }

    public async Task DeleteTableAsync(long tableId)
    {
        await repository.DeleteTableAsync(tableId);
    }

    /* ---------- Spalten ---------- */

    public async Task CreateColumnAsync(long tableId)
    {
        await repository.CreateColumnAsync(tableId);
    }

    public async Task UpdateColumnAsync(long columnId, UpdateColumnDto dto)
    {
        await repository.UpdateColumnAsync(columnId, dto);
    }

    public async Task UpdateColumnOrderAsync(List<UpdateColumnOrderDto> columns)
    {
        await repository.UpdateColumnOrderAsync(columns);
    }

    public async Task DeleteColumnAsync(long tableId, long columnId)
    {
        await repository.DeleteColumnAsync(tableId, columnId);
    }

    /* ---------- Rows ---------- */

    public async Task CreateRowAsync(long tableId)
    {
        await repository.CreateRowAsync(tableId);
    }

    public async Task UpdateRowOrderAsync(List<RowOrderUpdateDto> list)
    {
        await repository.UpdateRowOrderAsync(list);
    }

    public async Task DeleteRowsAsync(long tableId, List<long> rows)
    {
        await repository.DeleteRowsAsync(tableId, rows);
    }

    public async Task UpsertCellAsync(long rowId, long columnId, object? newValue)
    {
        await repository.UpsertCellAsync(rowId, columnId, newValue);
    }
}