using MultitoolApi.WebApi.Models.CustomTable;

public interface ICustomTableRepository
{
    Task<List<TableOverview>> GetTableListAsync();
    Task<TableDetail?> GetTableAsync(long tableId);
    Task CreateTableAsync(string name);
    Task UpdateTableAsync(long tableId, string newName);
    Task DeleteTableAsync(long tableId);
    Task<List<ColumnInfo>> GetColumnsAsync(long tableId);
    Task CreateColumnAsync(long tableId, CreateColumnDto dto);
    Task UpdateColumnAsync(long tableId, long columnId, UpdateColumnDto dto);
    Task DeleteColumnAsync(long tableId, long columnId);
    Task<List<RowInfo>> GetRowsAsync(long tableId, int pageNr, int pageSize);
    Task CreateRowAsync(long tableId, Dictionary<long, object?> cells);
    Task UpdateRowAsync(long tableId, long rowId, Dictionary<long, object?> cells);
    Task DeleteRowAsync(long tableId, long rowId);
    Task UpdateCellAsync(long rowId, long columnId, object? newValue);
}