using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Api.Tests.Controllers;

public class TodoControllerTests
{
    private readonly Mock<ITodoService> _todoServiceMock;

    private List<TodoDto> _getTodosResponse;
    private int _createTodoResponse;

    private static readonly int ID = TodoTestData.DefaultTodo.Id;

    public TodoControllerTests()
    {
        _todoServiceMock = new Mock<ITodoService>();

        // Default responses
        _getTodosResponse = new List<TodoDto>() { TodoTestData.DefaultTodoDto };
        _createTodoResponse = TodoTestData.DefaultTodo.Id;
    }

    private TodoController GetController()
    {
        _todoServiceMock.Reset();

        _todoServiceMock.Setup(s => s.GetTodosAsync())
            .ReturnsAsync(_getTodosResponse);

        _todoServiceMock.Setup(s => s.CreateTodoAsync(It.IsAny<CreateTodoDto>()))
            .ReturnsAsync(_createTodoResponse);

        _todoServiceMock.Setup(s => s.UpdateTodoAsync(It.IsAny<int>(), It.IsAny<UpdateTodoDto>()))
            .Returns(Task.CompletedTask);

        _todoServiceMock.Setup(s => s.DeleteTodoAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _todoServiceMock.Setup(s => s.ToggleDoneAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        return new TodoController(_todoServiceMock.Object);
    }

    // GET api/Todo

    [Fact]
    public async Task GetTodos_WhenTodosExist_ReturnsOkWithTodos()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetTodos();

        // Assert
        AssertEx.Ok(result, _getTodosResponse);

        _todoServiceMock.Verify(s => s.GetTodosAsync(), Times.Once);
        _todoServiceMock.VerifyNoOtherCalls();
    }

    // POST api/Todo

    [Fact]
    public async Task CreateTodo_WhenDtoIsValid_ReturnsCreatedWithId()
    {
        // Arrange
        var newTodo = TodoTestData.DefaultCreateTodoDto;
        var controller = GetController();

        // Act
        var result = await controller.CreateTodo(newTodo);

        // Assert
        AssertEx.Created(result, _createTodoResponse);

        _todoServiceMock.Verify(s => s.CreateTodoAsync(newTodo), Times.Once);
        _todoServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/Todo/{id}

    [Fact]
    public async Task UpdateTodo_WhenTodoExists_ReturnsNoContent()
    {
        // Arrange
        var newTodo = TodoTestData.DefaultUpdateTodoDto;
        var controller = GetController();

        // Act
        var result = await controller.UpdateTodo(ID, newTodo);

        // Assert
        AssertEx.NoContent(result);

        _todoServiceMock.Verify(s => s.UpdateTodoAsync(ID, newTodo), Times.Once);
        _todoServiceMock.VerifyNoOtherCalls();
    }

    // PATCH api/Todo/{id}/toggle

    [Fact]
    public async Task ToggleTodo_WhenTodoExists_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.ToggleTodo(ID);

        // Assert
        AssertEx.NoContent(result);

        _todoServiceMock.Verify(s => s.ToggleDoneAsync(ID), Times.Once);
        _todoServiceMock.VerifyNoOtherCalls();
    }

    // DELETE api/Todo/{id}

    [Fact]
    public async Task DeleteTodo_WhenTodoExists_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.DeleteTodo(ID);

        // Assert
        AssertEx.NoContent(result);

        _todoServiceMock.Verify(s => s.DeleteTodoAsync(ID), Times.Once);
        _todoServiceMock.VerifyNoOtherCalls();
    }
}