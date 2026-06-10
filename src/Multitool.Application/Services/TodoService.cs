using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Todo;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class TodoService(ITodoRepository todoRepository) : ITodoService
{
    public async Task<List<Todo>> GetAllTodosAsync()
    {
        return await todoRepository.GetAllAsync();
    }
}
