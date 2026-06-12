using Multitool.Domain.Entities.Todo;
using Multitool.Infrastructure.Repositories;
using Xunit;

namespace Multitool.Infrastructure.Tests;

public class TodoRepositoryTests : RepositoryTestBase
{
    private readonly TodoRepository _sut;
    private int _categoryId;

    public TodoRepositoryTests()
    {
        _sut = new TodoRepository(Context);
        SetupCategory().Wait();
    }

    private async Task SetupCategory()
    {
        var category = new Multitool.Domain.Entities.Category.Category { Name = "Test Category", Color = "#000000" };
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();
        _categoryId = category.Id;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSortedTodos()
    {
        // Arrange
        var t1 = new Todo { Title = "A", CategoryId = _categoryId, IsDone = true, CreationDateTime = DateTime.UtcNow.AddMinutes(-10) };
        var t2 = new Todo { Title = "B", CategoryId = _categoryId, IsDone = false, CreationDateTime = DateTime.UtcNow.AddMinutes(-5) };
        var t3 = new Todo { Title = "C", CategoryId = _categoryId, IsDone = false, CreationDateTime = DateTime.UtcNow, Priority = 1 };
        
        Context.Todos.AddRange(t1, t2, t3);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.False(result[0].IsDone); // t3 or t2
        Assert.True(result[2].IsDone); // t1 (done is last)
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTodo()
    {
        // Arrange
        var todo = new Todo { Title = "Test", CategoryId = _categoryId, IsDone = false };
        Context.Todos.Add(todo);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(todo.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(todo.Id, result.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldAddTodo()
    {
        // Arrange
        var todo = new Todo { Title = "New", CategoryId = _categoryId, IsDone = false };

        // Act
        await _sut.AddAsync(todo);

        // Assert
        var savedTodo = await Context.Todos.FindAsync(todo.Id);
        Assert.NotNull(savedTodo);
        Assert.Equal("New", savedTodo.Title);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTodo()
    {
        // Arrange
        var todo = new Todo { Title = "Old", CategoryId = _categoryId, IsDone = false };
        Context.Todos.Add(todo);
        await Context.SaveChangesAsync();
        Context.Entry(todo).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

        todo.Title = "Updated";

        // Act
        await _sut.UpdateAsync(todo);

        // Assert
        var updatedTodo = await Context.Todos.FindAsync(todo.Id);
        Assert.Equal("Updated", updatedTodo!.Title);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTodo()
    {
        // Arrange
        var todo = new Todo { Title = "To Delete", CategoryId = _categoryId, IsDone = false };
        Context.Todos.Add(todo);
        await Context.SaveChangesAsync();

        // Act
        await _sut.DeleteAsync(todo.Id);

        // Assert
        var deletedTodo = await Context.Todos.FindAsync(todo.Id);
        Assert.Null(deletedTodo);
    }
}