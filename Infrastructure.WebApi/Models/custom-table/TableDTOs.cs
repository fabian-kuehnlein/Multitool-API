namespace MultitoolApi.WebApi.Models.CustomTable;

public record TableOverview(long TableId, string Name);

public record CreateTableDto(string Name);
public record UpdateTableDto(string Name);

public record CustomTableResponseDto(
    long TableId,
    string Name,
    DateTime CreatedAt,
    List<CustomColumnResponseDto>? Columns = null,
    List<CustomRowResponseDto>? Rows = null
);
