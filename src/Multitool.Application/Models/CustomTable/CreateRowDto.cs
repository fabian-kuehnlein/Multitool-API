namespace Multitool.Application.Models.CustomTable;

public record CreateRowDto(
    Dictionary<long, object?> Cells
);
