namespace Multitool.Application.Models.Kanban;

public record CardModel(
    long Id,
    long ColumnId,
    string Title,
    string? Description,
    int Position,
    int CreatedByUserId,
    DateTime CreatedAt,
    List<int> CategoryIds);
