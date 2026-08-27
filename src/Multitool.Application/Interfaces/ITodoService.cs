using Multitool.Application.Models;

namespace Multitool.Application.Interfaces;

public interface ITodoService
{
    Task<List<TodoDto>> GetTodosAsync();
    Task<TodoDto?> GetTodoByIdAsync(int id);
    Task<int> CreateTodoAsync(CreateTodoDto createTodoDto);
    Task UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto);
    Task ToggleDoneAsync(int id);
    Task DeleteTodoAsync(int id);
    Task<int> DeletePastTodosAsync(int days);
}
