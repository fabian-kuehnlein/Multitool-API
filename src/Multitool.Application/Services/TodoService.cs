using Mapster;
using Multitool.Application.Extensions;
using Multitool.Application.Interfaces;
using Multitool.Application.Models;
using Multitool.Application.Models.Todo;
using Multitool.Domain.Entities.Todo;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class TodoService(ITodoRepository todoRepository, ICategoryRepository categoryRepository) : ITodoService
{
    public async Task<List<TodoModel>> GetTodosAsync()
    {
        var todos = await todoRepository.GetTodosAsync();
        return todos.Adapt<List<TodoModel>>();
    }

    public async Task<TodoModel?> GetTodoByIdAsync(int id)
    {
        var todo = await todoRepository.GetByIdAsync(id);
        return todo?.Adapt<TodoModel>();
    }

    public async Task<int> CreateTodoAsync(CreateTodoRequest request)
    {
        await categoryRepository.GetApplicableCategoryAsync(request.CategoryId, AppModule.Todo);

        var todo = request.Adapt<Todo>();

        return await todoRepository.CreateTodoAsync(todo);
    }

    public async Task UpdateTodoAsync(int id, UpdateTodoRequest request)
    {
        var existingTodo = await todoRepository.GetByIdAsync(id);
        if (existingTodo == null)
        {
            throw new NotFoundException($"Todo with ID {id} not found.");
        }

        await categoryRepository.GetApplicableCategoryAsync(request.CategoryId, AppModule.Todo);

        existingTodo.Title = request.Title;
        existingTodo.Description = request.Description;
        existingTodo.CategoryId = request.CategoryId;
        existingTodo.Priority = request.Priority;
        existingTodo.DueDate = request.DueDate;

        await todoRepository.UpdateTodoAsync(existingTodo);
    }

    public async Task SetDoneAsync(int id, bool isDone)
    {
        var todo = await todoRepository.GetByIdAsync(id);

        if (todo == null)
        {
            throw new NotFoundException($"Todo with ID {id} not found.");
        }

        todo.IsDone = isDone;
        todo.CompletedDateTime = isDone ? DateTime.Now : null;

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

    public async Task<int> DeletePastTodosAsync(int days)
    {
        var threshold = DateTime.Now.AddDays(-days);

        var todosToDelete = await todoRepository.GetTodosOlderThanAsync(threshold);

        foreach (var todo in todosToDelete)
        {
            await todoRepository.DeleteTodoAsync(todo);
        }

        return todosToDelete.Count;
    }
}
