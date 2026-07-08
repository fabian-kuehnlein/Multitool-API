namespace Multitool.Application.Models;

public record CreateTodoDto(
    string Title,
    string? Description,
    int CategoryId,
    int Priority,
    DateTime? DueDate);
