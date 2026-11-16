namespace Multitool.Application.Models.CustomTable;

public record RowOrderUpdateRequest(
    long RowId,
    int RowOrder
);
