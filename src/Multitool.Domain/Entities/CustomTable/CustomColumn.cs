using Multitool.Domain.Enums;

namespace Multitool.Domain.Entities.CustomTable;

public class CustomColumn
{
    public long ColumnId { get; set; }
    public long TableId { get; set; }
    public CustomTable Table { get; set; } = default!;
    public string Name { get; set; } = default!;
    public CustomDataType DataType { get; set; }
    public int ColOrder { get; set; }
}