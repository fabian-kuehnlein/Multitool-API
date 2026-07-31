using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Todo;
using Multitool.Tests.Shared;

namespace Multitool.Api.Tests.Controllers;

public class TodoControllerTests
{
    private readonly Mock<ITodoService> _serviceMock;
    private readonly TodoController _sut;

    private static readonly int DefaultTodoId = TodoTestData.DefaultTodo.Id;

    public TodoControllerTests()
    {
        _serviceMock = new Mock<ITodoService>();
        _sut = new TodoController(_serviceMock.Object);
    }

    // GET api/Todo

    [Fact]
    public async Task GetTodos_WhenTodosExist_ReturnsOkWithTodos()
    {
        // Arrange
        var todos = new List<Todo> { TodoTestData.DefaultTodo };
        _serviceMock.Setup(s => s.GetAllTodosAsync()).ReturnsAsync(todos);

        // Act
        var result = await _sut.GetTodos();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(todos);
    }

    // POST api/Todo

    [Fact]
    public async Task CreateTodo_WhenDtoIsValid_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = TodoTestData.DefaultCreateTodoDto;
        var createdTodo = TodoTestData.DefaultTodo;
        _serviceMock
            .Setup(s => s.CreateTodoAsync(dto))
            .ReturnsAsync(createdTodo);

        // Act
        var result = await _sut.CreateTodo(dto);

        // Assert
        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.Value.Should().BeEquivalentTo(createdTodo);
        created.ActionName.Should().Be(nameof(_sut.GetTodos));
    }

    // PUT api/Todo/{id}

    [Fact]
    public async Task UpdateTodo_WhenTodoExists_ReturnsNoContent()
    {
        // Arrange
        var dto = TodoTestData.DefaultUpdateTodoDto;
        _serviceMock.Setup(s => s.UpdateTodoAsync(DefaultTodoId, dto))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateTodo(DefaultTodoId, dto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    // DELETE api/Todo/{id}

    [Fact]
    public async Task DeleteTodo_WhenTodoExists_ReturnsNoContent()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteTodoAsync(DefaultTodoId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteTodo(DefaultTodoId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    // PATCH api/Todo/{id}/toggle

    [Fact]
    public async Task ToggleTodo_WhenTodoExists_ReturnsNoContent()
    {
        // Arrange
        _serviceMock.Setup(s => s.ToggleDoneAsync(DefaultTodoId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ToggleTodo(DefaultTodoId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}