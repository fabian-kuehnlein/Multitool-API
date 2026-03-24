using MultitoolApi.WebApi.Models.CustomTable;

namespace Multitool.Application.Interfaces;

public interface ICustomTableService
{
    Task<List<TableOverview>> GetTableListAsync();

    /* Tabelle CRUD */
    Task<TableDetail?> GetTableAsync(long tableId);
    Task<long> CreateTableAsync(CreateTableDto dto);
    Task UpdateTableAsync(long tableId, string name);
    Task DeleteTableAsync(long tableId);

    /* Spalten CRUD */
    Task CreateColumnAsync(long tableId);
    Task UpdateColumnAsync(long columnId, UpdateColumnDto dto);
    Task UpdateColumnOrderAsync(List<UpdateColumnOrderDto> columns);
    Task DeleteColumnAsync(long tableId, long columnId);

    /* Rows CRUD + Paging */
    Task CreateRowAsync(long tableId);
    Task UpdateRowOrderAsync(List<RowOrderUpdateDto> list);
    Task DeleteRowsAsync(long tableId, List<long> rows);

    /* Cell Updating */
    Task UpsertCellAsync(long rowId, long columnId, object? newValue);
}