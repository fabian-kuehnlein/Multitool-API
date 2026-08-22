using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Application.Models;
using Multitool.Domain.Entities.Todo;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class TodoService(ITodoRepository todoRepository) : ITodoService
{
    public async Task<List<TodoDto>> GetTodosAsync()
    {
        var todos = await todoRepository.GetTodosAsync();
        return todos.Adapt<List<TodoDto>>();
    }

    public async Task<TodoDto?> GetTodoByIdAsync(int id)
    {
        var todo = await todoRepository.GetByIdAsync(id);
        return todo?.Adapt<TodoDto>();
    }

    public async Task<int> CreateTodoAsync(CreateTodoDto createTodoDto)
    {
        var todo = createTodoDto.Adapt<Todo>();

        return await todoRepository.CreateTodoAsync(todo);
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

        await todoRepository.UpdateTodoAsync(existingTodo);
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

        await todoRepository.UpdateTodoAsync(todo);
    }

    public async Task DeleteTodoAsync(int id)
    {
        var existingTodo = await todoRepository.GetByIdAsync(id);

        if (existingTodo == null)
        {
            throw new NotFoundException($"Todo with ID {id} not found.");
        }

        await todoRepository.DeleteTodoAsync(existingTodo);
    }

    public async Task DeletePastTodosAsync(int days)
    {
        var threshold = DateTime.Now.AddDays(-days);

        var todosToDelete = await todoRepository.GetTodosOlderThanAsync(threshold);

        foreach (var todo in todosToDelete)
        {
            await todoRepository.DeleteTodoAsync(todo);
        }
    }
}
