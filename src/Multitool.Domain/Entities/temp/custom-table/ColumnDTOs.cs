

using Multitool.Domain.Enums;

namespace MultitoolApi.WebApi.Models.CustomTable;

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

public record ColumnInfo (long ColumnId, string ColumnName, CustomDataType DataType, int ColOrder);
