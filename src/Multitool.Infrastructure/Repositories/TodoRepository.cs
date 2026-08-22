using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Todo;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;

namespace Multitool.Infrastructure.Repositories;

public class TodoRepository(AppDbContext db) : ITodoRepository
{
    public async Task<List<Todo>> GetAllAsync()
    {
        return await db.Todos
            .AsNoTracking()
            .OrderBy(t => t.IsDone)
            .ThenBy(t => t.DueDate == null)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreationDateTime)
            .ToListAsync();
    }

    public async Task<Todo?> GetByIdAsync(int id)
    {
        return await db.Todos.FindAsync(id);
    }

    public async Task<int> CreateTodoAsync(Todo todo)
    {
        await db.Todos.AddAsync(todo);
        await db.SaveChangesAsync();

        return todo.Id;
    }

    public async Task UpdateTodoAsync(Todo todo)
    {
        db.Todos.Update(todo);
        await db.SaveChangesAsync();
    }

    public async Task DeleteTodoAsync(Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
    }

    public async Task<List<Todo>> GetTodosWithDueDateInRangeAsync(DateTime start, DateTime end)
        => await db.Todos
            .Where(t => t.DueDate != null &&
                        t.DueDate >= start &&
                        t.DueDate <= end &&
                        t.IsDone == false)
            .ToListAsync();

    public async Task<List<Todo>> GetTodosOlderThanAsync(DateTime date)
        => await db.Todos
            .Where(t => t.CompletedDateTime < date)
            .ToListAsync();
}
