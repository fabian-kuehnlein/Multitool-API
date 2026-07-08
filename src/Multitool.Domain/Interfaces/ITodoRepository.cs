using Multitool.Domain.Entities.Todo;

namespace Multitool.Domain.Interfaces;

public interface ITodoRepository
{
    Task<List<Todo>> GetAllAsync();
    Task<Todo?> GetByIdAsync(int id);
    Task AddAsync(Todo todo);
    Task UpdateAsync(Todo todo);
    Task DeleteAsync(int id);
    Task<List<Todo>> GetTodosWithDueDateInRangeAsync(DateTime start, DateTime end);
}