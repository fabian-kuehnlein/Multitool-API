using FluentAssertions;
using Moq;
using Multitool.Application.Interfaces;
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
        _repositoryMock = new Mock<ITodoRepository>();
        _sut = new TodoService(_repositoryMock.Object);
    }

    // GetAllTodosAsync

    [Fact]
    public async Task GetAllTodosAsync_WhenTodosExist_ReturnsTodos()
    {
        var todos = new List<Todo> { TodoTestData.DefaultTodo };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(todos);

        var result = await _sut.GetAllTodosAsync();

        result.Should().BeEquivalentTo(todos);
    }

    // GetTodoByIdAsync

    [Fact]
    public async Task GetTodoByIdAsync_WhenTodoExists_ReturnsTodo()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(TodoTestData.DefaultTodo.Id))
            .ReturnsAsync(TodoTestData.DefaultTodo);

        var result = await _sut.GetTodoByIdAsync(TodoTestData.DefaultTodo.Id);

        result.Should().BeEquivalentTo(TodoTestData.DefaultTodo);
    }

    [Fact]
    public async Task GetTodoByIdAsync_WhenTodoDoesNotExist_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Todo?)null);

        var result = await _sut.GetTodoByIdAsync(99);

        result.Should().BeNull();
    }

    // CreateTodoAsync

    [Fact]
    public async Task CreateTodoAsync_WhenDtoIsValid_ReturnsMappedTodo()
    {
        var dto = TodoTestData.DefaultCreateTodoDto;

        var result = await _sut.CreateTodoAsync(dto);

        result.Title.Should().Be(dto.Title);
        result.Description.Should().Be(dto.Description);
        result.CategoryId.Should().Be(dto.CategoryId);
        result.Priority.Should().Be(dto.Priority);
        result.DueDate.Should().Be(dto.DueDate);
    }

    [Fact]
    public async Task CreateTodoAsync_WhenDtoIsValid_SetsDoneToFalse()
    {
        var result = await _sut.CreateTodoAsync(TodoTestData.DefaultCreateTodoDto);

        result.IsDone.Should().BeFalse();
    }

    [Fact]
    public async Task CreateTodoAsync_WhenDtoIsValid_SetsCreationDateTimeToUtcNow()
    {
        var before = DateTime.UtcNow;

        var result = await _sut.CreateTodoAsync(TodoTestData.DefaultCreateTodoDto);

        result.CreationDateTime.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateTodoAsync_WhenDtoIsValid_CallsRepositoryAdd()
    {
        await _sut.CreateTodoAsync(TodoTestData.DefaultCreateTodoDto);

        _repositoryMock.Verify(r => r.AddAsync(It.Is<Todo>(t =>
            t.Title == TodoTestData.DefaultCreateTodoDto.Title)), Times.Once);
    }

    // UpdateTodoAsync

    [Fact]
    public async Task UpdateTodoAsync_WhenTodoExists_UpdatesAllFields()
    {
        var dto = TodoTestData.DefaultUpdateTodoDto;
        _repositoryMock.Setup(r => r.GetByIdAsync(TodoTestData.DefaultTodo.Id))
            .ReturnsAsync(TodoTestData.DefaultTodo);

        await _sut.UpdateTodoAsync(TodoTestData.DefaultTodo.Id, dto);

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
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Todo?)null);

        Func<Task> act = async () => await _sut.UpdateTodoAsync(99, TodoTestData.DefaultUpdateTodoDto);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*99*");
    }

    // ToggleDoneAsync

    [Fact]
    public async Task ToggleDoneAsync_WhenTodoIsFalse_SetsIsDoneToTrue()
    {
        var todo = TodoTestData.DefaultTodo;
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(todo);

        await _sut.ToggleDoneAsync(todo.Id);

        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Todo>(t => t.IsDone == true)), Times.Once);
    }

    [Fact]
    public async Task ToggleDoneAsync_WhenTodoIsTrue_SetsIsDoneToFalse()
    {
        var todo = TodoTestData.DefaultTodo;
        todo.IsDone = true;
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(todo);

        await _sut.ToggleDoneAsync(todo.Id);

        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Todo>(t => t.IsDone == false)), Times.Once);
    }

    [Fact]
    public async Task ToggleDoneAsync_WhenTodoDoesNotExist_ThrowsNotFoundException()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Todo?)null);

        Func<Task> act = async () => await _sut.ToggleDoneAsync(99);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*99*");
    }

    // DeleteTodoAsync

    [Fact]
    public async Task DeleteTodoAsync_WhenTodoExists_CallsRepositoryDelete()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(TodoTestData.DefaultTodo.Id))
            .ReturnsAsync(TodoTestData.DefaultTodo);

        await _sut.DeleteTodoAsync(TodoTestData.DefaultTodo.Id);

        _repositoryMock.Verify(r => r.DeleteAsync(TodoTestData.DefaultTodo.Id), Times.Once);
    }

    [Fact]
    public async Task DeleteTodoAsync_WhenTodoDoesNotExist_ThrowsNotFoundException()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Todo?)null);

        Func<Task> act = async () => await _sut.DeleteTodoAsync(99);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*99*");
    }
}