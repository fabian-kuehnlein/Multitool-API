using Microsoft.AspNetCore.Mvc;
using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models;
using Multitool.Domain.Entities.Todo;
using Xunit;

namespace Multitool.Api.Tests;

public class TodoControllerTests
{
    private readonly Mock<ITodoService> _serviceMock = new();
    private readonly TodoController _sut;

    public TodoControllerTests()
    {
        _sut = new TodoController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetTodos_ShouldReturnOk_WithTodos()
    {
        // Arrange
        var todos = new List<Todo> { new() { Id = 1, Title = "Test", CategoryId = 1, IsDone = false } };
        _serviceMock.Setup(x => x.GetAllTodosAsync()).ReturnsAsync(todos);

        // Act
        var result = await _sut.GetTodos();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(todos, okResult.Value);
    }

    [Fact]
    public async Task CreateTodo_ShouldReturnCreated()
    {
        // Arrange
        var dto = new CreateTodoDto("Title", "Desc", 1, 0, null);
        var createdTodo = new Todo { Id = 1, Title = "Title", CategoryId = 1, IsDone = false };
        _serviceMock.Setup(x => x.CreateTodoAsync(dto)).ReturnsAsync(createdTodo);

        // Act
        var result = await _sut.CreateTodo(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(createdTodo, createdResult.Value);
    }

    [Fact]
    public async Task UpdateTodo_ShouldReturnNoContent()
    {
        // Arrange
        var dto = new UpdateTodoDto("Title", "Desc", 1, 0, null);

        // Act
        var result = await _sut.UpdateTodo(1, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _serviceMock.Verify(x => x.UpdateTodoAsync(1, dto), Times.Once);
    }

    [Fact]
    public async Task ToggleTodo_ShouldReturnNoContent()
    {
        // Act
        var result = await _sut.ToggleTodo(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _serviceMock.Verify(x => x.ToggleDoneAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteTodo_ShouldReturnNoContent()
    {
        // Act
        var result = await _sut.DeleteTodo(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _serviceMock.Verify(x => x.DeleteTodoAsync(1), Times.Once);
    }
}