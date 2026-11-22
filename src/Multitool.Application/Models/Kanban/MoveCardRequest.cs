namespace Multitool.Application.Models.Kanban;

public record MoveCardRequest(long TargetColumnId, int Position);
