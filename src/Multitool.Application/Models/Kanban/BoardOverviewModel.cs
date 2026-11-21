namespace Multitool.Application.Models.Kanban;

public record BoardOverviewModel(
    long Id,
    string Name,
    DateTime CreatedAt,
    bool IsShared,
    int MemberCount);
