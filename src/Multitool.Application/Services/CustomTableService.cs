using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Application.Models;
using Multitool.Application.Models.CustomTable;
using Multitool.Domain.Entities.CustomTable;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class CustomTableService(ICustomTableRepository repository) : ICustomTableService
{
    public async Task<List<TableOverview>> GetTableListAsync()
    {
        var list = await repository.GetTableListAsync();
        return list.Adapt<List<TableOverview>>();
    }

    public async Task<TableDetail> GetTableAsync(long tableId)
    {
        var table = await repository.GetTableAsync(tableId);

        if (table == null)
            throw new NotFoundException($"Table with Id {tableId} not found");

        return table.Adapt<TableDetail>();
    }

    public async Task<long> CreateTableAsync(CreateTableDto dto)
    {
        var table = dto.Adapt<Table>();
        table.Columns.Add(dto.Column.Adapt<Column>());

        return await repository.CreateTableAsync(table);
    }

    public async Task UpdateTableAsync(long tableId, UpdateTableDto dto)
    {
        var existing = await repository.GetTableRawAsync(tableId);

        if (existing == null)
            throw new NotFoundException($"Table with Id {tableId} not found");

        existing.Name = dto.Name;

        await repository.UpdateTableAsync(existing);
    }

    public async Task DeleteTableAsync(long tableId)
    {
        var exists = await repository.TableExistsAsync(tableId);

        if (!exists)
            throw new NotFoundException($"Table with Id {tableId} not found");

        await repository.DeleteTableAsync(tableId);
    }

    public async Task CreateColumnAsync(long tableId)
    {
        var exists = await repository.TableExistsAsync(tableId);

        if (!exists)
            throw new NotFoundException($"Table with Id {tableId} not found");

        await repository.CreateColumnAsync(tableId);
    }

    public async Task UpdateColumnAsync(long columnId, UpdateColumnDto dto)
    {
        var existing = await repository.GetColumnAsync(columnId);

        if (existing == null)
            throw new NotFoundException($"Column with Id {columnId} not found");

        var typeChanged = existing.DataType != dto.DataType;

        existing.Name     = dto.Name;
        existing.DataType = dto.DataType;
        existing.ColOrder = dto.ColOrder;

        await repository.UpdateColumnAsync(existing, typeChanged);
    }

    public async Task UpdateColumnOrderAsync(List<UpdateColumnOrderDto> columns)
        => await repository.UpdateColumnOrderAsync(columns.Adapt<List<Column>>());

    public async Task DeleteColumnAsync(long tableId, long columnId)
    {
        var column = await repository.GetColumnAsync(columnId);

        if (column == null || column.TableId != tableId)
            throw new NotFoundException($"Column with Id {columnId} not found in table {tableId}");

        await repository.DeleteColumnAsync(columnId);
    }

    public async Task CreateRowAsync(long tableId)
    {
        var exists = await repository.TableExistsAsync(tableId);

        if (!exists)
            throw new NotFoundException($"Table with Id {tableId} not found");

        await repository.CreateRowAsync(tableId);
    }

    public async Task UpdateRowOrderAsync(List<RowOrderUpdateDto> rows)
        => await repository.UpdateRowOrderAsync(rows);

    public async Task DeleteRowsAsync(long tableId, List<long> rowIds)
    {
        var existingIds = await repository.GetExistingRowIdsAsync(tableId, rowIds);
        var missing = rowIds.Except(existingIds).ToList();

        if (missing.Count > 0)
            throw new NotFoundException($"Rows not found in table {tableId}: {string.Join(", ", missing)}");

        await repository.DeleteRowsAsync(tableId, rowIds);
    }

    public async Task UpsertCellAsync(long rowId, long columnId, object? newValue)
    {
        var row = await repository.GetRowAsync(rowId);
        if (row == null)
            throw new NotFoundException($"Row with Id {rowId} not found");

        var column = await repository.GetColumnAsync(columnId);
        if (column == null)
            throw new NotFoundException($"Column with Id {columnId} not found");

        await repository.UpsertCellAsync(rowId, columnId, column.DataType, newValue);
    }
}