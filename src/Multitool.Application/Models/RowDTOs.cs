namespace Multitool.Application.Models.CustomTable;

public record CreateRowDto(
    Dictionary<long, object?> Cells
);

public record UpdateRowDto(
    Dictionary<long, object?> Cells
);