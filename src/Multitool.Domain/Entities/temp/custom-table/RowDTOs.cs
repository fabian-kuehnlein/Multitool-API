namespace MultitoolApi.WebApi.Models.CustomTable;

public record CreateRowDto(
    Dictionary<long, object?> Cells
);

public record UpdateRowDto(
    Dictionary<long, object?> Cells
);

public record RowInfo(
    long RowId,
    Dictionary<long, object?> Cells,
    int RowOrder
);

public record RowOrderUpdateDto(
    int RowId,
    int RowOrder
);