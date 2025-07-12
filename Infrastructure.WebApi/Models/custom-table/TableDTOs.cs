namespace MultitoolApi.WebApi.Models.CustomTable;

public record TableOverview(long TableId, string Name);

public record TableDetail(
    long TableId,
    string Name,
    DateTime CreatedAt,
    List<ColumnInfo> Columns,
    List<RowInfo> Rows
);

public record CreateTableDto(string Name, CreateColumnDto Column);

public record CustomTableResponseDto(
    long TableId,
    string Name,
    DateTime CreatedAt,
    List<CustomColumnResponseDto>? Columns = null,
    List<CustomRowResponseDto>? Rows = null
);
