namespace Multitool.Application.Models.Kanban;

public record CreateBoardRequest(
    string Name,
    string? Description,
    List<string>? ColumnNames);
