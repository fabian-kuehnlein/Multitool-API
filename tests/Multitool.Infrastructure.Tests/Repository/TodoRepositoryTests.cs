using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Todo;
using Multitool.Infrastructure.Repositories;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Infrastructure.Tests;

public class TodoRepositoryTests : RepositoryTestBase
{
    private readonly TodoRepository _todoRepository;
    private int _categoryId;

    public TodoRepositoryTests()
    {
        _todoRepository = new TodoRepository(Context);
        SetupCategory();
    }

    private void SetupCategory()
    {
        var category = TodoTestData.DefaultCategory;
        Context.Categories.Add(category);
        Context.SaveChanges();
        _categoryId = category.Id;
    }

    private Todo CreateTodo(string title, bool isDone = false)
    {
        var todo = TodoTestData.DefaultTodo;
        todo.Id = 0;
        todo.Title = title;
        todo.CategoryId = _categoryId;
        todo.IsDone = isDone;
        return todo;
    }

    // GetTodosAsync

    [Fact]
    public async Task GetTodosAsync_WhenTodosExist_ReturnsSortedTodos()
    {
        // Arrange
        var t1 = CreateTodo("A", true);
        t1.CreationDateTime = DateTime.UtcNow.AddMinutes(-10);
        var t2 = CreateTodo("B");
        t2.CreationDateTime = DateTime.UtcNow.AddMinutes(-5);
        var t3 = CreateTodo("C");
        t3.CreationDateTime = DateTime.UtcNow;
        t3.Priority = 1;

        Context.Todos.AddRange(t1, t2, t3);
        await Context.SaveChangesAsync();

        // Act
        var result = await _todoRepository.GetTodosAsync();

        // Assert
        AssertEx.AreEqual(3, result.Count);
        Assert.False(result[0].IsDone);
        Assert.True(result[2].IsDone);
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenTodoExists_ReturnsTodo()
    {
        // Arrange
        var todo = CreateTodo("Test");
        Context.Todos.Add(todo);
        await Context.SaveChangesAsync();

        // Act
        var result = await _todoRepository.GetByIdAsync(todo.Id);

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(result!.Id, todo.Id);
    }

    // CreateTodoAsync

    [Fact]
    public async Task CreateTodoAsync_WhenTodoIsValid_AddsTodo()
    {
        // Arrange
        var todo = CreateTodo("New");

        // Act
        await _todoRepository.CreateTodoAsync(todo);

        // Assert
        var savedTodo = await Context.Todos.AsNoTracking().FirstOrDefaultAsync(t => t.Id == todo.Id);
        Assert.NotNull(savedTodo);
        AssertEx.AreEqual("New", savedTodo.Title);
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenTodoExists_UpdatesTodo()
    {
        // Arrange
        var todo = CreateTodo("Old");
        Context.Todos.Add(todo);
        await Context.SaveChangesAsync();

        Context.Entry(todo).State = EntityState.Detached;

        todo.Title = "Updated";

        // Act
        await _todoRepository.UpdateTodoAsync(todo);

        // Assert
        var updatedTodo = await Context.Todos.AsNoTracking().FirstOrDefaultAsync(t => t.Id == todo.Id);
        AssertEx.AreEqual("Updated", updatedTodo!.Title);
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_WhenTodoExists_RemovesTodo()
    {
        // Arrange
        var todo = CreateTodo("To Delete");
        Context.Todos.Add(todo);
        await Context.SaveChangesAsync();

        // Act
        await _todoRepository.DeleteTodoAsync(todo);

        // Assert
        var deletedTodo = await Context.Todos.FindAsync(todo.Id);
        Assert.Null(deletedTodo);
    }
}