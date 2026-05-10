namespace Multitool.Application.Models.CustomTable;

public record TableOverview(long TableId, string Name);

public record CreateTableDto(
    string Name,
    CreateColumnDto Column
);

public record UpdateTableDto(
    string Name
);
