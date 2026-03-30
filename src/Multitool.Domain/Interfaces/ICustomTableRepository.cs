using Multitool.Domain.Entities.CustomTable;

namespace Multitool.Domain.Interfaces;

public interface ICustomTableRepository
{
    Task<List<Table>> GetTableListAsync();
    Task<Table> GetTableAsync(long tableId);
    Task<long> CreateTableAsync(Table table, Column column);
    Task UpdateTableAsync(long tableId, string newName);
    Task DeleteTableAsync(long tableId);
    Task CreateColumnAsync(long tableId);
    Task UpdateColumnAsync(Column column);
    Task UpdateColumnOrderAsync(List<Column> columns);
    Task DeleteColumnAsync(long tableId, long columnId);
    Task CreateRowAsync(long tableId);
    Task UpdateRowOrderAsync(List<Row> list);
    Task DeleteRowsAsync(long tableId, List<long> rowIds);
    Task UpsertCellAsync(long rowId, long columnId, object? value);
}