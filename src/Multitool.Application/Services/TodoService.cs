using Multitool.Application.Interfaces;
using Multitool.Application.Models;
using Multitool.Domain.Entities.Todo;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class TodoService(ITodoRepository todoRepository) : ITodoService
{
    public async Task<List<Todo>> GetAllTodosAsync()
    {
        return await todoRepository.GetAllAsync();
    }

    public async Task<Todo?> GetTodoByIdAsync(int id)
    {
        return await todoRepository.GetByIdAsync(id);
    }

    public async Task<Todo> CreateTodoAsync(CreateTodoDto createTodoDto)
    {
        var todo = new Todo
        {
            Title = createTodoDto.Title,
            Description = createTodoDto.Description,
            CategoryId = createTodoDto.CategoryId,
            Priority = createTodoDto.Priority,
            DueDate = createTodoDto.DueDate,
            IsDone = false,
            CreationDateTime = DateTime.Now
        };

        await todoRepository.AddAsync(todo);
        return todo;
    }

    public async Task UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto)
    {
        var existingTodo = await todoRepository.GetByIdAsync(id);
        if (existingTodo == null)
        {
            throw new NotFoundException($"Todo with ID {id} not found.");
        }

        existingTodo.Title = updateTodoDto.Title;
        existingTodo.Description = updateTodoDto.Description;
        existingTodo.CategoryId = updateTodoDto.CategoryId;
        existingTodo.Priority = updateTodoDto.Priority;
        existingTodo.DueDate = updateTodoDto.DueDate;

        await todoRepository.UpdateAsync(existingTodo);
    }

    public async Task ToggleDoneAsync(int id)
    {
        var todo = await todoRepository.GetByIdAsync(id);
        if (todo == null)
        {
            throw new NotFoundException($"Todo with ID {id} not found.");
        }

        todo.IsDone = !todo.IsDone;
        todo.CompletedDateTime = todo.IsDone ? DateTime.Now : null;

        await todoRepository.UpdateAsync(todo);
    }

    public async Task DeleteTodoAsync(int id)
    {
        var existingTodo = await todoRepository.GetByIdAsync(id);
        if (existingTodo == null)
        {
            throw new NotFoundException($"Todo with ID {id} not found.");
        }

        await todoRepository.DeleteAsync(id);
    }

    public async Task DeletePastTodosAsync(int days)
    {
        var threshold = DateTime.Now.AddDays(-days);

        var todos = await todoRepository.GetTodosOlderThanAsync(threshold);

        foreach (var todo in todos)
        {
            await todoRepository.DeleteAsync(todo.Id);
        }
    }
}
