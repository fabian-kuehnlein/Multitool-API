using Multitool.Domain.Enums;

namespace Multitool.Application.Models;

public record ColumnInfo
{
    public long ColumnId { get; init; }
    public string Name { get; init; } = default!;
    public CustomDataType DataType { get; init; }
    public int ColOrder { get; init; }
}