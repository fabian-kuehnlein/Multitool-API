namespace Multitool.Domain.Entities.CustomTable;

public class Table
{
    public long TableId { get; set; }
    public string Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public List<Column> Columns { get; set; } = new List<Column>();
    public List<Row> Rows { get; set; } = new List<Row>();
}