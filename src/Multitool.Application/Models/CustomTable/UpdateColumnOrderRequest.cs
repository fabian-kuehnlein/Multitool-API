namespace Multitool.Application.Models.CustomTable;

public record UpdateColumnOrderRequest(
    long ColumnId,
    int ColOrder
);
