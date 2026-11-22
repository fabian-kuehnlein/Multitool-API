namespace Multitool.Application.Models.Kanban;

public record BoardOverviewModel(
    long Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    int MemberCount);
