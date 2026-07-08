namespace Multitool.Application.Models;

public record UpdateTodoDto(
    string Title,
    string? Description,
    int CategoryId,
    int Priority,
    DateTime? DueDate);
