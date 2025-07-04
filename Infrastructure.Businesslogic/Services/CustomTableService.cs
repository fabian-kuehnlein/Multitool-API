using AutoMapper;
using MultitoolApi.Businesslogic.Models;
using MultitoolApi.DataAccessLayer.Models;
using MultitoolApi.WebApi.Models.CustomTable;

namespace MultitoolApi.Infrastructure.Businesslogic.Services;

public class CustomTableService : ICustomTableService
{
    private readonly ICustomTableRepository _repository;
    private readonly IMapper _mapper;

    public CustomTableService(ICustomTableRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    
    public async Task<IReadOnlyList<TableOverview>> GetTableListAsync()
    {
        return await _repository.GetTableListAsync();
    }
}