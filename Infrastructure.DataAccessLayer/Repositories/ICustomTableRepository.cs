using MultitoolApi.WebApi.Models.CustomTable;

public interface ICustomTableRepository
{
    Task<List<TableOverview>> GetTableListAsync();
    Task<TableDetail?> GetTableAsync(long tableId);
    Task<long> CreateTableAsync(CreateTableDto dto);
    Task UpdateTableAsync(long tableId, string newName);
    Task DeleteTableAsync(long tableId);
    Task CreateColumnAsync(long tableId);
    Task UpdateColumnAsync(long tableId, long columnId, UpdateColumnDto dto);
    Task DeleteColumnAsync(long tableId, long columnId);
    Task CreateRowAsync(long tableId);
    Task DeleteRowsAsync(long tableId, List<long> rows);
    Task UpsertCellAsync(long rowId, long columnId, object? value);
}