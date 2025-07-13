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
    Task CreateColumnAsync(long tableId);
    Task UpdateColumnAsync(long tableId, long columnId, UpdateColumnDto dto);
    Task DeleteColumnAsync(long tableId, long columnId);

    /* Rows CRUD + Paging */
    Task CreateRowAsync(long tableId);

    Task DeleteRowsAsync(long tableId, List<long> rows);

    /* Cell Updating */
    Task UpsertCellAsync(long rowId, long columnId, object? newValue);
}