using Multitool.Domain.Entities.Todo;

namespace Multitool.Domain.Interfaces;

public interface ITodoRepository
{
    Task<List<Todo>> GetAllAsync();
}