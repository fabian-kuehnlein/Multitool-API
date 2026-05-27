namespace Multitool.Domain.Entities.CustomTable;

public record RowOrderUpdateDto(
    long RowId,
    int RowOrder
);