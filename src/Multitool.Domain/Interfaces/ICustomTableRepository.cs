using Multitool.Domain.Entities.CustomTable;
using Multitool.Domain.Enums;

namespace Multitool.Domain.Interfaces;

public interface ICustomTableRepository
{
    Task<List<Table>> GetTableListAsync();
    Task<Table?> GetTableAsync(long tableId);
    Task<long> CreateTableAsync(Table table);
    Task UpdateTableAsync(Table table);
    Task DeleteTableAsync(long tableId);
    Task CreateColumnAsync(long tableId);
    Task UpdateColumnAsync(Column column, bool typeChanged);
    Task UpdateColumnOrderAsync(List<Column> columns);
    Task DeleteColumnAsync(long columnId);
    Task CreateRowAsync(long tableId);
    Task UpdateRowOrderAsync(Dictionary<long, int> rowOrders);
    Task DeleteRowsAsync(long tableId, List<long> rowIds);
    Task UpsertCellAsync(long rowId, long columnId, CustomDataType dataType, object? value);

    Task<Table?> GetTableRawAsync(long tableId);
    Task<Column?> GetColumnAsync(long columnId);
    Task<Row?> GetRowAsync(long rowId);
    Task<bool> TableExistsAsync(long tableId);
    Task<List<long>> GetExistingRowIdsAsync(long tableId, List<long> rowIds);
}