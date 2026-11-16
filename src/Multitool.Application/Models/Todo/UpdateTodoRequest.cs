namespace Multitool.Application.Models.Todo;

public record UpdateTodoRequest(
    string Title,
    string? Description,
    int CategoryId,
    int Priority,
    DateTime? DueDate);
