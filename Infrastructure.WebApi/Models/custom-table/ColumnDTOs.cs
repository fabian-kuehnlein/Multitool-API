using MultitoolApi.Infrastructure.DataAccessLayer.Models.CustomTable;

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

public record CustomColumnResponseDto(
    long ColumnId,
    string Name,
    CustomDataType DataType,
    int ColOrder
);

public record ColumnInfo (long ColumnId, string ColumnName, CustomDataType DataType, int ColOrder);
