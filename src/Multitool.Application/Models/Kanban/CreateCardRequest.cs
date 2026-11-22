namespace Multitool.Application.Models.Kanban;

public record CreateCardRequest(
    long ColumnId,
    string Title,
    string? Description,
    List<int>? CategoryIds);
