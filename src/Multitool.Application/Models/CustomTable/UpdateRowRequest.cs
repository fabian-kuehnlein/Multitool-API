namespace Multitool.Application.Models.CustomTable;

public record UpdateRowRequest(
    Dictionary<long, object?> Cells
);
