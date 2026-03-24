namespace Multitool.Domain.Entities.CustomTable;

public class CustomRow
{
    public long RowId { get; set; }
    public long TableId { get; set; }
    public CustomTable Table { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public ICollection<CustomCell> Cells { get; set; } = [];
    public int RowOrder { get; set; }
}