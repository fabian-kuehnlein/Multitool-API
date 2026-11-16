namespace Multitool.Application.Models.CustomTable;

public record CreateTableRequest(
    string Name,
    CreateColumnRequest Column
);
