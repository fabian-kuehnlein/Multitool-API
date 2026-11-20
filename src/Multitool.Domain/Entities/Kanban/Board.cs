using Multitool.Domain.Entities.Config;

namespace Multitool.Domain.Entities.Kanban;

public class Board
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<User> Members { get; set; } = []; // Skip Navigation → board_members
    public List<BoardColumn> Columns { get; set; } = [];
}
