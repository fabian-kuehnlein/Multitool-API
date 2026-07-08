using Multitool.Application.Models;
using Multitool.Domain.Entities.Todo;
using Multitool.Domain.Entities.Category;

namespace Multitool.Tests.Shared;

public static class TodoTestData
{
    public static Todo DefaultTodo => new()
    {
        Id = 1,
        Title = "Test Todo",
        Description = "Test Description",
        CategoryId = 1,
        IsDone = false,
        Priority = 1,
        DueDate = new DateTime(2026, 6, 11, 12, 0, 0, DateTimeKind.Utc),
        CreationDateTime = new DateTime(2026, 6, 11, 10, 0, 0, DateTimeKind.Utc)
    };

    public static readonly CreateTodoDto DefaultCreateTodoDto = new(
        "Test Todo",
        "Test Description",
        1,
        1,
        new DateTime(2026, 6, 11, 12, 0, 0, DateTimeKind.Utc)
    );

    public static readonly UpdateTodoDto DefaultUpdateTodoDto = new(
        "Updated Todo",
        "Updated Description",
        2,
        2,
        new DateTime(2026, 6, 12, 12, 0, 0, DateTimeKind.Utc)
    );

    public static readonly Category DefaultCategory = new() { Id = 1, Name = "Test Category", Color = "#000000" };
    public static readonly Category SecondCategory = new() { Id = 2, Name = "Second Category", Color = "#FFFFFF" };
}
