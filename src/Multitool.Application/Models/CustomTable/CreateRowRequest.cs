namespace Multitool.Application.Models.CustomTable;

public record CreateRowRequest(
    Dictionary<long, object?> Cells
);
