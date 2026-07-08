using Multitool.Application.Models;
using Multitool.Domain.Entities.Todo;

namespace Multitool.Application.Interfaces;

public interface ITodoService
{
    Task<List<Todo>> GetAllTodosAsync();
    Task<Todo?> GetTodoByIdAsync(int id);
    Task<Todo> CreateTodoAsync(CreateTodoDto createTodoDto);
    Task UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto);
    Task ToggleDoneAsync(int id);
    Task DeleteTodoAsync(int id);
}
