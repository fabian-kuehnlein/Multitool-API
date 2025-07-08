using MultitoolApi.DataAccessLayer.Models;
using MultitoolApi.WebApi.Models.CustomTable;

namespace MultitoolApi.Infrastructure.Businesslogic.Services;

public interface ICustomTableService
{
    Task<List<TableOverview>> GetTableListAsync();

    /* Tabelle CRUD */
    Task<TableDetail?> GetTableAsync(long tableId);
    Task<long> CreateTableAsync(CreateTableDto dto);
    Task UpdateTableAsync(long tableId, string name);
    Task DeleteTableAsync(long tableId);

    /* Spalten CRUD */
    Task<List<ColumnInfo>> GetColumnsAsync(long tableId);
    Task CreateColumnAsync(long tableId);
    Task UpdateColumnAsync(long tableId, long columnId, UpdateColumnDto dto);
    Task DeleteColumnAsync(long tableId, long columnId);

    /* Rows CRUD + Paging */
    Task<List<RowInfo>> GetRowsAsync(long tableId, int pageNr, int pageSize);
    Task CreateRowAsync(long tableId);
    Task UpdateRowAsync(long tableId, long rowId, Dictionary<long, object?> cells);
    Task DeleteRowAsync(long tableId, long rowId);

    /* Cell Updating */
    Task UpsertCellAsync(long rowId, long columnId, object? newValue);
}