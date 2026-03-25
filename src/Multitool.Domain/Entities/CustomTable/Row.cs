namespace Multitool.Domain.Entities.CustomTable;

public class Row
{
    public long RowId { get; set; }
    public long TableId { get; set; }
    public Table Table { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public ICollection<Cell> Cells { get; set; } = [];
    public int RowOrder { get; set; }
}