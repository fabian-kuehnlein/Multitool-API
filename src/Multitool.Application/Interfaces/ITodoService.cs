using Multitool.Domain.Entities.Todo;

namespace Multitool.Application.Interfaces;

public interface ITodoService
{
    Task<List<Todo>> GetAllTodosAsync();
}
