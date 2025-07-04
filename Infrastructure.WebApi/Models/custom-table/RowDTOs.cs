namespace MultitoolApi.WebApi.Models.CustomTable;

public record CreateRowDto(
    Dictionary<long, object?> Cells // ColumnId → Value
);

public record UpdateRowDto(
    Dictionary<long, object?> Cells // ColumnId → Value
);

public record CustomRowResponseDto(
    long RowId,
    Dictionary<long, object?> Cells
);
