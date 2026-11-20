namespace Multitool.Domain.Entities.Kanban;

public class BoardColumn
{
    public long Id { get; set; }
    public long BoardId { get; set; }
    public Board Board { get; set; } = default!;
    public required string Name { get; set; }
    public int Position { get; set; }
    public List<Card> Cards { get; set; } = [];
}
