using FluentAssertions;
using Mapster;
using Moq;
using Multitool.Application.Interfaces;
using Multitool.Application.Mappings;
using Multitool.Application.Models;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Todo;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Xunit;

namespace Multitool.Application.Tests;

public class TodoServiceTests
{
    private readonly Mock<ITodoRepository> _repositoryMock;
    private readonly TodoService _sut;

    public TodoServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());
        _repositoryMock = new Mock<ITodoRepository>();
        _sut = new TodoService(_repositoryMock.Object);
    }

    // GetAllTodosAsync

    [Fact]
    public async Task GetAllTodosAsync_WhenTodosExist_ReturnsTodos()
    {
        // Arrange
        var todos = new List<Todo> { TodoTestData.DefaultTodo };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(todos);

        // Act
        var result = await _sut.GetAllTodosAsync();

        // Assert
        result.Should().BeEquivalentTo(todos.Adapt<List<TodoDto>>());
    }

    // GetTodoByIdAsync

    [Fact]
    public async Task GetTodoByIdAsync_WhenTodoExists_ReturnsTodo()
    {
        // Arrange
        var todo = TodoTestData.DefaultTodo;
        _repositoryMock.Setup(r => r.GetByIdAsync(todo.Id))
            .ReturnsAsync(todo);

        // Act
        var result = await _sut.GetTodoByIdAsync(todo.Id);

        // Assert
        result.Should().BeEquivalentTo(todo.Adapt<TodoDto>());
    }

    [Fact]
    public async Task GetTodoByIdAsync_WhenTodoDoesNotExist_ReturnsNull()
    {
        // Arrange
        const int todoId = 99;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Todo?)null);

        // Act
        var result = await _sut.GetTodoByIdAsync(todoId);

        // Assert
        result.Should().BeNull();
    }

    // CreateTodoAsync

    [Fact]
    public async Task CreateTodoAsync_WhenDtoIsValid_ReturnsMappedTodo()
    {
        // Arrange
        var dto = TodoTestData.DefaultCreateTodoDto;

        // Act
        var result = await _sut.CreateTodoAsync(dto);

        // Assert
        result.Title.Should().Be(dto.Title);
        result.Description.Should().Be(dto.Description);
        result.CategoryId.Should().Be(dto.CategoryId);
        result.Priority.Should().Be(dto.Priority);
        result.DueDate.Should().Be(dto.DueDate);
    }

    [Fact]
    public async Task CreateTodoAsync_WhenDtoIsValid_SetsDoneToFalse()
    {
        // Arrange

        // Act
        var result = await _sut.CreateTodoAsync(TodoTestData.DefaultCreateTodoDto);

        // Assert
        result.IsDone.Should().BeFalse();
    }

    [Fact]
    public async Task CreateTodoAsync_WhenDtoIsValid_SetsCreationDateTimeToUtcNow()
    {
        // Arrange
        var before = DateTime.Now;

        // Act
        var result = await _sut.CreateTodoAsync(TodoTestData.DefaultCreateTodoDto);

        // Assert
        result.CreationDateTime.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.Now);
    }

    [Fact]
    public async Task CreateTodoAsync_WhenDtoIsValid_CallsRepositoryAdd()
    {
        // Arrange

        // Act
        await _sut.CreateTodoAsync(TodoTestData.DefaultCreateTodoDto);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Todo>(t =>
            t.Title == TodoTestData.DefaultCreateTodoDto.Title)), Times.Once);
    }

    // UpdateTodoAsync

    [Fact]
    public async Task UpdateTodoAsync_WhenTodoExists_UpdatesAllFields()
    {
        // Arrange
        var todo = TodoTestData.DefaultTodo;
        var dto = TodoTestData.DefaultUpdateTodoDto;
        _repositoryMock.Setup(r => r.GetByIdAsync(todo.Id))
            .ReturnsAsync(todo);

        // Act
        await _sut.UpdateTodoAsync(todo.Id, dto);

        // Assert
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Todo>(t =>
            t.Title == dto.Title &&
            t.Description == dto.Description &&
            t.CategoryId == dto.CategoryId &&
            t.Priority == dto.Priority &&
            t.DueDate == dto.DueDate)), Times.Once);
    }

    [Fact]
    public async Task UpdateTodoAsync_WhenTodoDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int todoId = 99;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Todo?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateTodoAsync(todoId, TodoTestData.DefaultUpdateTodoDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*99*");
    }

    // ToggleDoneAsync

    [Fact]
    public async Task ToggleDoneAsync_WhenTodoIsFalse_SetsIsDoneToTrue()
    {
        // Arrange
        var todo = TodoTestData.DefaultTodo;
        _repositoryMock.Setup(r => r.GetByIdAsync(todo.Id)).ReturnsAsync(todo);

        // Act
        await _sut.ToggleDoneAsync(todo.Id);

        // Assert
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Todo>(t => t.IsDone == true)), Times.Once);
    }

    [Fact]
    public async Task ToggleDoneAsync_WhenTodoIsTrue_SetsIsDoneToFalse()
    {
        // Arrange
        var todo = TodoTestData.DefaultTodo;
        todo.IsDone = true;
        _repositoryMock.Setup(r => r.GetByIdAsync(todo.Id)).ReturnsAsync(todo);

        // Act
        await _sut.ToggleDoneAsync(todo.Id);

        // Assert
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Todo>(t => t.IsDone == false)), Times.Once);
    }

    [Fact]
    public async Task ToggleDoneAsync_WhenTodoDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int todoId = 99;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Todo?)null);

        // Act
        Func<Task> act = async () => await _sut.ToggleDoneAsync(todoId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*99*");
    }

    // DeleteTodoAsync

    [Fact]
    public async Task DeleteTodoAsync_WhenTodoExists_CallsRepositoryDelete()
    {
        // Arrange
        var todo = TodoTestData.DefaultTodo;
        _repositoryMock.Setup(r => r.GetByIdAsync(todo.Id))
            .ReturnsAsync(todo);

        // Act
        await _sut.DeleteTodoAsync(todo.Id);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(todo.Id), Times.Once);
    }

    [Fact]
    public async Task DeleteTodoAsync_WhenTodoDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int todoId = 99;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Todo?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteTodoAsync(todoId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*99*");
    }

    // DeletePastTodosAsync

    [Fact]
    public async Task DeletePastTodosAsync_WhenTodosExist_DeletesEachTodo()
    {
        // Arrange
        var pastTodos = new List<Todo>
        {
            new() { Id = 1, Title = "Old 1", CategoryId = 1, IsDone = false },
            new() { Id = 2, Title = "Old 2", CategoryId = 1, IsDone = false }
        };
        _repositoryMock.Setup(r => r.GetTodosOlderThanAsync(It.IsAny<DateTime>())).ReturnsAsync(pastTodos);

        // Act
        await _sut.DeletePastTodosAsync(30);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(2), Times.Once);
    }

    [Fact]
    public async Task DeletePastTodosAsync_WhenNoTodosExist_DoesNotCallDelete()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetTodosOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Todo>());

        // Act
        await _sut.DeletePastTodosAsync(30);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}