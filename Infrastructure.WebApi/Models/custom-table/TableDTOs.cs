namespace MultitoolApi.WebApi.Models.CustomTable;

public record TableOverview(long TableId, string Name);

public record TableDetail(
    long TableId,
    string Name,
    DateTime CreatedAt,
    List<ColumnInfo> Columns,
    List<RowInfo> Rows
);

public record CreateTableDto(
    string Name,
    CreateColumnDto Column
);
