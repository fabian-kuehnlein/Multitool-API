namespace Multitool.Application.Models.Kanban;

public record BoardModel(
    long Id,
    string Name,
    string? Description,
    bool IsOwner,
    List<ColumnModel> Columns,
    List<BoardMemberModel> Members);
