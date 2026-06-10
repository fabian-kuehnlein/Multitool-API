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
}
