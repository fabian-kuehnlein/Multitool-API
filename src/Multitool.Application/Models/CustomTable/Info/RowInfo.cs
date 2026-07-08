namespace Multitool.Application.Models.Info;

public record RowInfo
{
    public long RowId { get; init; }
    public Dictionary<long, object?> Cells { get; init; } = new();
    public int RowOrder { get; init;}
}