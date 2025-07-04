using MultitoolApi.DataAccessLayer.Models;
using MultitoolApi.WebApi.Models.CustomTable;

namespace MultitoolApi.Infrastructure.Businesslogic.Services;

public interface ICustomTableService
{
    Task<IReadOnlyList<TableOverview>> GetTableListAsync();
}