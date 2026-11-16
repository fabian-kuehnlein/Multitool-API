namespace Multitool.Application.Models.Todo;

public record CreateTodoRequest(
    string Title,
    string? Description,
    int CategoryId,
    int Priority,
    DateTime? DueDate);
