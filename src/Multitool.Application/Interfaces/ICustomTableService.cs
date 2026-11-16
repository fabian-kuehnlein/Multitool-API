using Multitool.Application.Models.CustomTable;
using Multitool.Application.Models.Info;

namespace Multitool.Application.Interfaces;

public interface ICustomTableService
{
    Task<List<TableOverviewModel>> GetTableListAsync();
    Task<TableDetail> GetTableAsync(long tableId);
    Task<long> CreateTableAsync(CreateTableRequest request);
    Task UpdateTableAsync(long tableId, UpdateTableRequest request);
    Task DeleteTableAsync(long tableId);
    Task CreateColumnAsync(long tableId);
    Task UpdateColumnAsync(long columnId, UpdateColumnRequest request);
    Task UpdateColumnOrderAsync(List<UpdateColumnOrderRequest> columns);
    Task DeleteColumnAsync(long tableId, long columnId);
    Task CreateRowAsync(long tableId);
    Task UpdateRowOrderAsync(List<RowOrderUpdateRequest> list);
    Task DeleteRowsAsync(long tableId, List<long> rows);
    Task UpsertCellAsync(long rowId, long columnId, object? newValue);
}