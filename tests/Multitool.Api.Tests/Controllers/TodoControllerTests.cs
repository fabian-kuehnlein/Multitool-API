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

    public TodoControllerTests()
    {
        _serviceMock = new Mock<ITodoService>();
        _sut = new TodoController(_serviceMock.Object);
    }

    // GET api/Todo

    [Fact]
    public async Task GetTodos_WhenTodosExist_ReturnsOkWithTodos()
    {
        var todos = new List<Todo> { TodoTestData.DefaultTodo };
        _serviceMock.Setup(s => s.GetAllTodosAsync()).ReturnsAsync(todos);

        var result = await _sut.GetTodos();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(todos);
    }

    // POST api/Todo

    [Fact]
    public async Task CreateTodo_WhenDtoIsValid_ReturnsCreatedAtAction()
    {
        _serviceMock
            .Setup(s => s.CreateTodoAsync(TodoTestData.DefaultCreateTodoDto))
            .ReturnsAsync(TodoTestData.DefaultTodo);

        var result = await _sut.CreateTodo(TodoTestData.DefaultCreateTodoDto);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.Value.Should().BeEquivalentTo(TodoTestData.DefaultTodo);
        created.ActionName.Should().Be(nameof(_sut.GetTodos));
    }

    // PUT api/Todo/{id}

    [Fact]
    public async Task UpdateTodo_WhenTodoExists_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.UpdateTodoAsync(TodoTestData.DefaultTodo.Id, TodoTestData.DefaultUpdateTodoDto))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateTodo(TodoTestData.DefaultTodo.Id, TodoTestData.DefaultUpdateTodoDto);

        result.Should().BeOfType<NoContentResult>();
    }

    // DELETE api/Todo/{id}

    [Fact]
    public async Task DeleteTodo_WhenTodoExists_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.DeleteTodoAsync(TodoTestData.DefaultTodo.Id))
            .Returns(Task.CompletedTask);

        var result = await _sut.DeleteTodo(TodoTestData.DefaultTodo.Id);

        result.Should().BeOfType<NoContentResult>();
    }

    // PATCH api/Todo/{id}/toggle

    [Fact]
    public async Task ToggleTodo_WhenTodoExists_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.ToggleDoneAsync(TodoTestData.DefaultTodo.Id))
            .Returns(Task.CompletedTask);

        var result = await _sut.ToggleTodo(TodoTestData.DefaultTodo.Id);

        result.Should().BeOfType<NoContentResult>();
    }
}