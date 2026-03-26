namespace Multitool.Application.Models;

public record TableDetail
{
    public long TableId { get; init; }
    public string Name { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
    public List<ColumnInfo> Columns { get; init; } = [];
    public List<RowInfo> Rows { get; init; } = [];
}