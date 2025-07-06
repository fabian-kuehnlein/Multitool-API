using MultitoolApi.DataAccessLayer.Models;
using MultitoolApi.WebApi.Models.CustomTable;

namespace MultitoolApi.Infrastructure.Businesslogic.Services;

public interface ICustomTableService
{
    Task<List<TableOverview>> GetTableListAsync();

    /* Tabelle CRUD */
    Task<TableDetail?> GetTableAsync(long tableId);
    Task CreateTableAsync(string name);
    Task UpdateTableAsync(long tableId, string name);
    Task DeleteTableAsync(long tableId);

    /* Spalten CRUD */
    Task<List<ColumnInfo>> GetColumnsAsync(long tableId);
    Task CreateColumnAsync(long tableId, CreateColumnDto dto);
    Task UpdateColumnAsync(long tableId, long columnId, UpdateColumnDto dto);
    Task DeleteColumnAsync(long tableId, long columnId);

    /* Rows CRUD + Paging */
    Task<List<RowInfo>> GetRowsAsync(long tableId, int pageNr, int pageSize);
    Task CreateRowAsync(long tableId, Dictionary<long, object?> cells);
    Task UpdateRowAsync(long tableId, long rowId, Dictionary<long, object?> cells);
    Task DeleteRowAsync(long tableId, long rowId);

    /* Cell Updating */
    Task UpdateCellAsync(long rowId, long columnId, object? newValue);
}