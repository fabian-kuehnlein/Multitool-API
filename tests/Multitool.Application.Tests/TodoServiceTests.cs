using Moq;
using Multitool.Application.Interfaces;
using Multitool.Application.Models;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Todo;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Xunit;

namespace Multitool.Application.Tests;

public class TodoServiceTests
{
    private readonly Mock<ITodoRepository> _repositoryMock = new();
    private readonly ITodoService _sut;

    public TodoServiceTests()
    {
        _sut = new TodoService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllTodosAsync_ShouldReturnTodos()
    {
        // Arrange
        var todos = new List<Todo> { new() { Id = 1, Title = "Test", CategoryId = 1, IsDone = false } };
        _repositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(todos);

        // Act
        var result = await _sut.GetAllTodosAsync();

        // Assert
        Assert.Equal(todos, result);
    }

    [Fact]
    public async Task GetTodoByIdAsync_ShouldReturnTodo()
    {
        // Arrange
        var todo = new Todo { Id = 1, Title = "Test", CategoryId = 1, IsDone = false };
        _repositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(todo);

        // Act
        var result = await _sut.GetTodoByIdAsync(1);

        // Assert
        Assert.Equal(todo, result);
    }

    [Fact]
    public async Task CreateTodoAsync_ShouldSetIsDoneToFalseAndCreationDate()
    {
        // Arrange
        var dto = new CreateTodoDto("Test", "Desc", 1, 0, null);

        // Act
        var result = await _sut.CreateTodoAsync(dto);

        // Assert
        Assert.False(result.IsDone);
        Assert.NotEqual(default, result.CreationDateTime);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Todo>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTodoAsync_ShouldUpdateFields_WhenTodoExists()
    {
        // Arrange
        var existingTodo = new Todo { Id = 1, Title = "Old", CategoryId = 1, IsDone = true };
        var updateDto = new UpdateTodoDto("New", "Desc", 2, 5, DateTime.MaxValue);
        
        _repositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingTodo);

        // Act
        await _sut.UpdateTodoAsync(1, updateDto);

        // Assert
        Assert.Equal("New", existingTodo.Title);
        Assert.Equal("Desc", existingTodo.Description);
        Assert.Equal(2, existingTodo.CategoryId);
        Assert.Equal(5, existingTodo.Priority);
        Assert.Equal(DateTime.MaxValue, existingTodo.DueDate);
        Assert.True(existingTodo.IsDone); // Should NOT be updated
        _repositoryMock.Verify(x => x.UpdateAsync(existingTodo), Times.Once);
    }

    [Fact]
    public async Task UpdateTodoAsync_ShouldThrowNotFound_WhenTodoDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Todo?)null);
        var updateDto = new UpdateTodoDto("New", "Desc", 2, 5, DateTime.MaxValue);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateTodoAsync(1, updateDto));
    }

    [Fact]
    public async Task ToggleDoneAsync_ShouldToggleStatus()
    {
        // Arrange
        var todo = new Todo { Id = 1, Title = "Test", CategoryId = 1, IsDone = false };
        _repositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(todo);

        // Act
        await _sut.ToggleDoneAsync(1);

        // Assert
        Assert.True(todo.IsDone);
        _repositoryMock.Verify(x => x.UpdateAsync(todo), Times.Once);

        // Act again
        await _sut.ToggleDoneAsync(1);

        // Assert
        Assert.False(todo.IsDone);
    }

    [Fact]
    public async Task DeleteTodoAsync_ShouldCallRepository_WhenTodoExists()
    {
        // Arrange
        var todo = new Todo { Id = 1, Title = "Test", CategoryId = 1, IsDone = false };
        _repositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(todo);

        // Act
        await _sut.DeleteTodoAsync(1);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteTodoAsync_ShouldThrowNotFound_WhenTodoDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Todo?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteTodoAsync(1));
    }
}