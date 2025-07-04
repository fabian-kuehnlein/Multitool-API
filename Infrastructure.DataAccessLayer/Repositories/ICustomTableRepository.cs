using MultitoolApi.WebApi.Models.CustomTable;

public interface ICustomTableRepository
{
    Task<IReadOnlyList<TableOverview>> GetTableListAsync(CancellationToken cancellationToken = default);
}