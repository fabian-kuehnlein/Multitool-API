using Multitool.Application.Models;
using Multitool.Application.Models.Todo;

namespace Multitool.Application.Interfaces;

public interface ITodoService
{
    Task<List<TodoModel>> GetTodosAsync();
    Task<TodoModel?> GetTodoByIdAsync(int id);
    Task<int> CreateTodoAsync(CreateTodoRequest request);
    Task UpdateTodoAsync(int id, UpdateTodoRequest request);
    Task SetDoneAsync(int id, bool isDone);
    Task DeleteTodoAsync(int id);
    Task<int> DeletePastTodosAsync(int days);
}
