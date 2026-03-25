namespace Multitool.Domain.Entities.CustomTable;

public class Table
{
    public long TableId { get; set; }
    public string Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public ICollection<Column> Columns { get; set; } = [];
    public ICollection<Row> Rows { get; set; } = [];
}