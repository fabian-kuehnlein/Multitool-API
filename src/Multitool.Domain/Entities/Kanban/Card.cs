using Multitool.Domain.Entities.Categories;

namespace Multitool.Domain.Entities.Kanban;

public class Card
{
    public long Id { get; set; }
    public long ColumnId { get; set; }
    public BoardColumn Column { get; set; } = default!;
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Position { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Category> Categories { get; set; } = []; // Skip Navigation → card_categories
}
