using Mapster;
using Moq;
using Multitool.Application.Mappings;
using Multitool.Application.Models;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Todo;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class TodoServiceTests
{
    private readonly Mock<ITodoRepository> _repositoryMock;

    private List<Todo> _getTodosResponse;
    private Todo? _getByIdResponse;
    private int _createTodoResponse;

    private static readonly int ID = TodoTestData.DefaultTodo.Id;

    private Todo? _createdTodo;
    private Todo? _updatedTodo;
    private Todo? _deletedTodo;

    public TodoServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());

        _repositoryMock = new Mock<ITodoRepository>();

        _getTodosResponse = new List<Todo>();
        _getByIdResponse = TodoTestData.DefaultTodo;
        _createTodoResponse = ID;
    }

    private TodoService GetService()
    {
        _createdTodo = null;
        _updatedTodo = null;
        _deletedTodo = null;

        _repositoryMock.Reset();

        _repositoryMock.Setup(r => r.GetTodosAsync())
            .ReturnsAsync(_getTodosResponse);

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(_getByIdResponse);

        _repositoryMock.Setup(r => r.CreateTodoAsync(It.IsAny<Todo>()))
            .Callback<Todo>(t => _createdTodo = t)
            .ReturnsAsync(_createTodoResponse);

        _repositoryMock.Setup(r => r.UpdateTodoAsync(It.IsAny<Todo>()))
            .Callback<Todo>(t => _updatedTodo = t)
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.DeleteTodoAsync(It.IsAny<Todo>()))
            .Callback<Todo>(t => _deletedTodo = t)
            .Returns(Task.CompletedTask);

        return new TodoService(_repositoryMock.Object);
    }

    // GetTodosAsync
    [Fact]
    public async Task GetTodosAsync_WhenTodosExist_ReturnsTodos()
    {
        // Arrange
        _getTodosResponse = new List<Todo> { TodoTestData.DefaultTodo };
        var service = GetService();

        // Act
        var result = await service.GetTodosAsync();

        // Assert
        AssertEx.AreEqual(result, _getTodosResponse.Adapt<List<TodoDto>>());

        _repositoryMock.Verify(r => r.GetTodosAsync(), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // GetTodoByIdAsync
    [Fact]
    public async Task GetTodoByIdAsync_WhenTodoExists_ReturnsTodo()
    {
        // Arrange
        _getByIdResponse = TodoTestData.DefaultTodo;
        var service = GetService();

        // Act
        var result = await service.GetTodoByIdAsync(ID);

        // Assert
        AssertEx.AreEqual(result, _getByIdResponse.Adapt<TodoDto>());

        _repositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetTodoByIdAsync_WhenTodoDoesNotExist_ReturnsNull()
    {
        // Arrange
        _getByIdResponse = null;
        var service = GetService();

        // Act
        var result = await service.GetTodoByIdAsync(ID);

        // Assert
        AssertEx.AreEqual(result, null);

        _repositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // CreateTodoAsync
    [Fact]
    public async Task CreateTodoAsync_WhenDtoIsValid_CallsRepositoryAdd()
    {
        // Arrange
        var todo = TodoTestData.DefaultCreateTodoDto;
        var service = GetService();

        // Act
        var result = await service.CreateTodoAsync(todo);

        // Assert
        AssertEx.AreEqual(result, ID);
        AssertEx.AreEqual(_createdTodo, new Todo
        {
            Title = todo.Title,
            Description = todo.Description,
            CategoryId = todo.CategoryId,
            Priority = todo.Priority,
            DueDate = todo.DueDate,
            IsDone = false,
            CreationDateTime = _createdTodo!.CreationDateTime
        });
        AssertEx.CloseTo(_createdTodo!.CreationDateTime, DateTime.Now, TimeSpan.FromSeconds(5));

        _repositoryMock.Verify(r => r.CreateTodoAsync(It.Is<Todo>(createdTodo =>
            createdTodo.Title == todo.Title &&
            createdTodo.Description == todo.Description &&
            createdTodo.CategoryId == todo.CategoryId &&
            createdTodo.Priority == todo.Priority &&
            createdTodo.DueDate == todo.DueDate &&
            createdTodo.IsDone == false &&
            createdTodo.CreationDateTime <= DateTime.Now
        )), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // UpdateTodoAsync
    [Fact]
    public async Task UpdateTodoAsync_WhenTodoExists_UpdatesAllFields()
    {
        // Arrange
        var todo = TodoTestData.DefaultTodo;
        var dto = TodoTestData.DefaultUpdateTodoDto;

        _getByIdResponse = todo;
        var service = GetService();

        // Act
        await service.UpdateTodoAsync(todo.Id, dto);

        // Assert
        AssertEx.AreEqual(_updatedTodo, new Todo
        {
            Id = todo.Id,
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            IsDone = todo.IsDone,
            CreationDateTime = todo.CreationDateTime
        });

        _repositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _repositoryMock.Verify(r => r.UpdateTodoAsync(It.Is<Todo>(t =>
            t.Title == dto.Title &&
            t.Description == dto.Description &&
            t.CategoryId == dto.CategoryId &&
            t.Priority == dto.Priority &&
            t.DueDate == dto.DueDate
        )), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateTodoAsync_WhenTodoDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getByIdResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateTodoAsync(ID, TodoTestData.DefaultUpdateTodoDto);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _repositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _repositoryMock.Verify(r => r.UpdateTodoAsync(It.IsAny<Todo>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // ToggleDoneAsync
    [Fact]
    public async Task ToggleDoneAsync_WhenTodoIsFalse_SetsIsDoneToTrue()
    {
        // Arrange
        var todo = TodoTestData.DefaultTodo;
        todo.IsDone = false;
        _getByIdResponse = todo;

        var service = GetService();

        // Act
        await service.ToggleDoneAsync(todo.Id);

        // Assert
        AssertEx.AreEqual(_updatedTodo, new Todo
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            CategoryId = todo.CategoryId,
            Priority = todo.Priority,
            DueDate = todo.DueDate,
            IsDone = true,
            CreationDateTime = todo.CreationDateTime,
            CompletedDateTime = _updatedTodo!.CompletedDateTime
        });
        AssertEx.CloseTo(_updatedTodo.CompletedDateTime!.Value, DateTime.Now, TimeSpan.FromSeconds(5));

        _repositoryMock.Verify(r => r.GetByIdAsync(todo.Id), Times.Once);
        _repositoryMock.Verify(r => r.UpdateTodoAsync(It.Is<Todo>(t => t.IsDone == true)), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    // DeleteTodoAsync
    [Fact]
    public async Task DeleteTodoAsync_WhenTodoExists_CallsRepositoryDelete()
    {
        // Arrange
        var todo = TodoTestData.DefaultTodo;
        _getByIdResponse = todo;

        var service = GetService();

        // Act
        await service.DeleteTodoAsync(todo.Id);

        // Assert
        AssertEx.AreEqual(_deletedTodo, todo);

        _repositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _repositoryMock.Verify(r => r.DeleteTodoAsync(todo), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }
}
