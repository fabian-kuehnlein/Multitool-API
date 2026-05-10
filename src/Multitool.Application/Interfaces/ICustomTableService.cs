using Multitool.Application.Models;
using Multitool.Application.Models.CustomTable;
using Multitool.Domain.Entities.CustomTable;

namespace Multitool.Application.Interfaces;

public interface ICustomTableService
{
    Task<List<TableOverview>> GetTableListAsync();
    Task<TableDetail> GetTableAsync(long tableId);
    Task<long> CreateTableAsync(CreateTableDto dto);
    Task UpdateTableAsync(long tableId, UpdateTableDto dto);
    Task DeleteTableAsync(long tableId);
    Task CreateColumnAsync(long tableId);
    Task UpdateColumnAsync(long columnId, UpdateColumnDto dto);
    Task UpdateColumnOrderAsync(List<UpdateColumnOrderDto> columns);
    Task DeleteColumnAsync(long tableId, long columnId);
    Task CreateRowAsync(long tableId);
    Task UpdateRowOrderAsync(List<RowOrderUpdateDto> list);
    Task DeleteRowsAsync(long tableId, List<long> rows);
    Task UpsertCellAsync(long rowId, long columnId, object? newValue);
}