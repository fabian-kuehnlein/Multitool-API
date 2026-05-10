

using Multitool.Domain.Enums;

namespace Multitool.Application.Models.CustomTable;

public record CreateColumnDto(
    string Name,
    CustomDataType DataType,
    int ColOrder
);

public record UpdateColumnDto(
    string Name,
    CustomDataType DataType,
    int ColOrder
);

public record UpdateColumnOrderDto(
    long ColumnId,
    int ColOrder
);
