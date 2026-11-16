using Multitool.Application.Models.CustomTable;
using Multitool.Domain.Entities.CustomTable;
using Multitool.Domain.Enums;
using Multitool.Application.Models.Info;

namespace Multitool.Tests.Shared;

public static class CustomTableTestData
{
    public static Table DefaultTable => new()
    {
        TableId = 1,
        UserId = AuthTestData.DefaultUser.Id,
        Name = "Test Table",
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    public static Column DefaultColumn => new()
    {
        ColumnId = 1,
        TableId = 1,
        Name = "Test Column",
        DataType = CustomDataType.String,
        ColOrder = 0
    };

    public static Row DefaultRow => new()
    {
        RowId = 1,
        TableId = 1,
        RowOrder = 0,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    public static TableOverviewModel DefaultTableOverview => new(1, "Test Table");

    public static TableDetail DefaultTableDetail => new()
    {
        TableId = 1,
        Name = "Test Table",
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        Columns = [new ColumnInfo { ColumnId = 1, Name = "Test Column", DataType = CustomDataType.String, ColOrder = 0 }],
        Rows = [new RowInfo { RowId = 1, RowOrder = 0, Cells = [] }]
    };

    public static CreateTableRequest DefaultCreateTableRequest => new(
        "New Table",
        new CreateColumnRequest("First Col", CustomDataType.String, 0)
    );

    public static UpdateTableRequest DefaultUpdateTableRequest => new("Updated Table Name");

    public static UpdateColumnRequest DefaultUpdateColumnRequest => new("Updated Column Name", CustomDataType.Int, 1);
}
