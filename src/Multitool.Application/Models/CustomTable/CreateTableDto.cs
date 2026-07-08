namespace Multitool.Application.Models.CustomTable;

public record CreateTableDto(
    string Name,
    CreateColumnDto Column
);
