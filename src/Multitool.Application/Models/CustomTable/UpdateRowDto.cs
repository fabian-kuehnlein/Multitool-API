namespace Multitool.Application.Models.CustomTable;

public record UpdateRowDto(
    Dictionary<long, object?> Cells
);
