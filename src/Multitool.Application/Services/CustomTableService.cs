using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.CustomTable;
using Multitool.Domain.Interfaces;
using MultitoolApi.WebApi.Models.CustomTable;

namespace Multitool.Application.Services;

public class CustomTableService(ICustomTableRepository repository) : ICustomTableService
{
    public async Task<List<TableOverview>> GetTableListAsync()
        => await repository.GetTableListAsync();

    public async Task<TableDetail?> GetTableAsync(long tableId)
        => await repository.GetTableAsync(tableId);

    public async Task<long> CreateTableAsync(CreateTableDto dto)
    {
        var table = dto.Adapt<Table>();
        var column = dto.Adapt<Column>();

        return await repository.CreateTableAsync(table, column);
    }

    public async Task UpdateTableAsync(long tableId, string name)
        => await repository.UpdateTableAsync(tableId, name);

    public async Task DeleteTableAsync(long tableId)
        => await repository.DeleteTableAsync(tableId);

    public async Task CreateColumnAsync(long tableId)
        => await repository.CreateColumnAsync(tableId);

    public async Task UpdateColumnAsync(long columnId, UpdateColumnDto dto)
    {
        var column = dto.Adapt<Column>();
        column.ColumnId = columnId;

        await repository.UpdateColumnAsync(column);
    }

    public async Task UpdateColumnOrderAsync(List<UpdateColumnOrderDto> columns)
        => await repository.UpdateColumnOrderAsync(columns.Adapt<List<Column>>());

    public async Task DeleteColumnAsync(long tableId, long columnId)
        => await repository.DeleteColumnAsync(tableId, columnId);

    public async Task CreateRowAsync(long tableId)
        => await repository.CreateRowAsync(tableId);

    public async Task UpdateRowOrderAsync(List<RowOrderUpdateDto> rows)
        => await repository.UpdateRowOrderAsync(rows.Adapt<List<Row>>());

    public async Task DeleteRowsAsync(long tableId, List<long> rows)
        => await repository.DeleteRowsAsync(tableId, rows);

    public async Task UpsertCellAsync(long rowId, long columnId, object? newValue)
        => await repository.UpsertCellAsync(rowId, columnId, newValue);
}