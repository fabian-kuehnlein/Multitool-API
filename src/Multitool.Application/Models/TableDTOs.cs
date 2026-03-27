namespace MultitoolApi.WebApi.Models.CustomTable;

public record TableOverview(long TableId, string Name);

public record CreateTableDto(
    string Name,
    CreateColumnDto Column
);
