using Multitool.Domain.Entities.Todo;

namespace Multitool.Domain.Interfaces;

public interface ITodoRepository
{
    Task<List<Todo>> GetAllAsync();
    Task<Todo?> GetByIdAsync(int id);
    Task<int> CreateTodoAsync(Todo todo);
    Task UpdateTodoAsync(Todo todo);
    Task DeleteTodoAsync(Todo todo);
    Task<List<Todo>> GetTodosWithDueDateInRangeAsync(DateTime start, DateTime end);
    Task<List<Todo>> GetTodosOlderThanAsync(DateTime date);
}