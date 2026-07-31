using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.CustomTable;
using Multitool.Domain.Enums;
using Multitool.Infrastructure.Repositories;

namespace Multitool.Infrastructure.Tests;

public class CustomTableRepositoryTests : RepositoryTestBase
{
    private readonly CustomTableRepository _sut;

    public CustomTableRepositoryTests()
    {
        _sut = new CustomTableRepository(Context);
    }

    // GetTableListAsync

    [Fact]
    public async Task GetTableListAsync_WhenTablesExist_ReturnsAllTablesSortedByName()
    {
        // Arrange
        Context.CustomTables.AddRange(
            new Table { Name = "B" },
            new Table { Name = "A" }
        );
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetTableListAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("A");
        result[1].Name.Should().Be("B");
    }

    // GetTableAsync

    [Fact]
    public async Task GetTableAsync_WhenTableExists_ReturnsTableWithColumnsAndRows()
    {
        // Arrange
        var table = new Table { Name = "Complex Table" };
        table.Columns.Add(new Column { Name = "Col 1", DataType = CustomDataType.String, ColOrder = 0 });
        table.Rows.Add(new Row { RowOrder = 0 });
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetTableAsync(table.TableId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Complex Table");
        result.Columns.Should().HaveCount(1);
        result.Rows.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetTableAsync_WhenTableDoesNotExist_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _sut.GetTableAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // CreateTableAsync

    [Fact]
    public async Task CreateTableAsync_WhenTableIsValid_AddsTableToDatabase()
    {
        // Arrange
        var table = new Table { Name = "Test Table" };

        // Act
        var id = await _sut.CreateTableAsync(table);

        // Assert
        id.Should().BeGreaterThan(0);
        var dbTable = await Context.CustomTables.FindAsync(id);
        dbTable.Should().NotBeNull();
        dbTable!.Name.Should().Be("Test Table");
    }

    // UpdateTableAsync

    [Fact]
    public async Task UpdateTableAsync_WhenTableExists_UpdatesName()
    {
        // Arrange
        var table = new Table { Name = "Old" };
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        table.Name = "New";

        // Act
        await _sut.UpdateTableAsync(table);

        // Assert
        var dbTable = await Context.CustomTables.FindAsync(table.TableId);
        dbTable!.Name.Should().Be("New");
    }

    // DeleteTableAsync

    [Fact]
    public async Task DeleteTableAsync_WhenTableExists_RemovesTableAndRelatedData()
    {
        // Arrange
        var table = new Table { Name = "To Delete" };
        var col = new Column { Name = "Col", DataType = CustomDataType.String, ColOrder = 0 };
        table.Columns.Add(col);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteTableAsync(table.TableId);

        // Assert
        var dbTable = await Context.CustomTables.FindAsync(table.TableId);
        dbTable.Should().BeNull();
        var dbCol = await Context.CustomColumns.FindAsync(col.ColumnId);
        dbCol.Should().BeNull();
    }

    // CreateColumnAsync

    [Fact]
    public async Task CreateColumnAsync_WhenColumnsExist_AddsColumnWithNextOrder()
    {
        // Arrange
        var table = new Table { Name = "Table" };
        table.Columns.Add(new Column { Name = "Col 0", DataType = CustomDataType.String, ColOrder = 0 });
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.CreateColumnAsync(table.TableId);

        // Assert
        var dbCols = await Context.CustomColumns.Where(c => c.TableId == table.TableId).ToListAsync();
        dbCols.Should().HaveCount(2);
        dbCols.Should().Contain(c => c.Name == "Neue Spalte" && c.ColOrder == 1);
    }

    // UpdateColumnAsync

    [Fact]
    public async Task UpdateColumnAsync_WhenTypeChanged_DeletesCells()
    {
        // Arrange
        var table = new Table { Name = "Table" };
        var col = new Column { Name = "Col", DataType = CustomDataType.String, ColOrder = 0 };
        table.Columns.Add(col);
        var row = new Row { RowOrder = 0 };
        table.Rows.Add(row);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        var cell = new Cell { RowId = row.RowId, ColumnId = col.ColumnId, ValString = "Data" };
        Context.CustomCells.Add(cell);
        await Context.SaveChangesAsync();

        col.DataType = CustomDataType.Int;

        // Act
        await _sut.UpdateColumnAsync(col, true);

        // Assert
        var dbCells = await Context.CustomCells.Where(c => c.ColumnId == col.ColumnId).ToListAsync();
        dbCells.Should().BeEmpty();
    }

    // UpdateColumnOrderAsync

    [Fact]
    public async Task UpdateColumnOrderAsync_WhenColumnsExist_UpdatesOrders()
    {
        // Arrange
        var table = new Table { Name = "Table" };
        var c1 = new Column { Name = "C1", DataType = CustomDataType.String, ColOrder = 0 };
        var c2 = new Column { Name = "C2", DataType = CustomDataType.String, ColOrder = 1 };
        table.Columns.AddRange(c1, c2);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        c1.ColOrder = 1;
        c2.ColOrder = 0;

        // Act
        await _sut.UpdateColumnOrderAsync(new List<Column> { c1, c2 });

        // Assert
        var dbC1 = await Context.CustomColumns.FindAsync(c1.ColumnId);
        var dbC2 = await Context.CustomColumns.FindAsync(c2.ColumnId);
        dbC1!.ColOrder.Should().Be(1);
        dbC2!.ColOrder.Should().Be(0);
    }

    // DeleteColumnAsync

    [Fact]
    public async Task DeleteColumnAsync_WhenColumnExists_RemovesColumn()
    {
        // Arrange
        var table = new Table { Name = "Table" };
        var col = new Column { Name = "Col", DataType = CustomDataType.String, ColOrder = 0 };
        table.Columns.Add(col);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteColumnAsync(col.ColumnId);

        // Assert
        var dbCol = await Context.CustomColumns.FindAsync(col.ColumnId);
        dbCol.Should().BeNull();
    }

    // CreateRowAsync

    [Fact]
    public async Task CreateRowAsync_WhenRowsExist_AddsRowWithNextOrder()
    {
        // Arrange
        var table = new Table { Name = "Table" };
        table.Rows.Add(new Row { RowOrder = 0 });
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.CreateRowAsync(table.TableId);

        // Assert
        var dbRows = await Context.CustomRows.Where(r => r.TableId == table.TableId).ToListAsync();
        dbRows.Should().HaveCount(2);
        dbRows.Should().Contain(r => r.RowOrder == 1);
    }

    // UpdateRowOrderAsync

    [Fact]
    public async Task UpdateRowOrderAsync_WhenRowsExist_UpdatesOrders()
    {
        // Arrange
        var table = new Table { Name = "Table" };
        var r1 = new Row { RowOrder = 0 };
        var r2 = new Row { RowOrder = 1 };
        table.Rows.AddRange(r1, r2);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        var rowOrderMap = new Dictionary<long, int>
        {
            { r1.RowId, 1 },
            { r2.RowId, 0 }
        };

        // Act
        await _sut.UpdateRowOrderAsync(rowOrderMap);

        // Assert
        var dbR1 = await Context.CustomRows.FindAsync(r1.RowId);
        var dbR2 = await Context.CustomRows.FindAsync(r2.RowId);
        dbR1!.RowOrder.Should().Be(1);
        dbR2!.RowOrder.Should().Be(0);
    }

    // DeleteRowsAsync

    [Fact]
    public async Task DeleteRowsAsync_WhenRowIdsProvided_RemovesSpecificRows()
    {
        // Arrange
        var table = new Table { Name = "Table" };
        var r1 = new Row { RowOrder = 0 };
        var r2 = new Row { RowOrder = 1 };
        table.Rows.AddRange(r1, r2);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        var rowIds = new List<long> { r1.RowId };

        // Act
        await _sut.DeleteRowsAsync(table.TableId, rowIds);

        // Assert
        var dbRows = await Context.CustomRows.Where(r => r.TableId == table.TableId).ToListAsync();
        dbRows.Should().HaveCount(1);
        dbRows[0].RowId.Should().Be(r2.RowId);
    }

    // UpsertCellAsync

    [Fact]
    public async Task UpsertCellAsync_WhenCellExists_UpdatesValue()
    {
        // Arrange
        var table = new Table { Name = "Table" };
        var col = new Column { Name = "Col", DataType = CustomDataType.String, ColOrder = 0 };
        table.Columns.Add(col);
        var row = new Row { RowOrder = 0 };
        table.Rows.Add(row);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.UpsertCellAsync(row.RowId, col.ColumnId, CustomDataType.String, "New Value");

        // Assert
        var dbCell = await Context.CustomCells.FindAsync(row.RowId, col.ColumnId);
        dbCell.Should().NotBeNull();
        dbCell!.ValString.Should().Be("New Value");
    }

    [Fact]
    public async Task UpsertCellAsync_WhenCellDoesNotExist_CreatesNewCell()
    {
        // Arrange
        var table = new Table { Name = "Table" };
        var col = new Column { Name = "Col", DataType = CustomDataType.String, ColOrder = 0 };
        table.Columns.Add(col);
        var row = new Row { RowOrder = 0 };
        table.Rows.Add(row);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.UpsertCellAsync(row.RowId, col.ColumnId, CustomDataType.String, "First Value");

        // Assert
        var dbCell = await Context.CustomCells.FindAsync(row.RowId, col.ColumnId);
        dbCell.Should().NotBeNull();
        dbCell!.ValString.Should().Be("First Value");
    }

    [Theory]
    [InlineData(CustomDataType.Int, "42")]
    [InlineData(CustomDataType.Decimal, "3.14")]
    [InlineData(CustomDataType.Bool, "true")]
    public async Task UpsertCellAsync_WhenValueMatchesDataType_StoresParsedValue(CustomDataType dataType, string value)
    {
        // Arrange
        var table = new Table { Name = "Table" };
        var col = new Column { Name = "Col", DataType = dataType, ColOrder = 0 };
        table.Columns.Add(col);
        var row = new Row { RowOrder = 0 };
        table.Rows.Add(row);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.UpsertCellAsync(row.RowId, col.ColumnId, dataType, value);

        // Assert
        var dbCell = await Context.CustomCells.FindAsync(row.RowId, col.ColumnId);
        dbCell.Should().NotBeNull();

        switch (dataType)
        {
            case CustomDataType.Int:
                dbCell!.ValInt.Should().Be(42);
                break;
            case CustomDataType.Decimal:
                dbCell!.ValDec.Should().Be(3.14m);
                break;
            case CustomDataType.Bool:
                dbCell!.ValBool.Should().BeTrue();
                break;
        }
    }

    [Fact]
    public async Task UpsertCellAsync_WhenDateValueIsValid_StoresDate()
    {
        // Arrange
        var table = new Table { Name = "Table" };
        var col = new Column { Name = "Col", DataType = CustomDataType.Date, ColOrder = 0 };
        table.Columns.Add(col);
        var row = new Row { RowOrder = 0 };
        table.Rows.Add(row);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.UpsertCellAsync(row.RowId, col.ColumnId, CustomDataType.Date, "2026-06-12");

        // Assert
        var dbCell = await Context.CustomCells.FindAsync(row.RowId, col.ColumnId);
        dbCell!.ValDate.Should().Be(new DateTime(2026, 6, 12));
    }

    [Fact]
    public async Task UpsertCellAsync_WhenValueDoesNotMatchDataType_ThrowsArgumentException()
    {
        // Arrange
        var table = new Table { Name = "Table" };
        var col = new Column { Name = "Col", DataType = CustomDataType.Int, ColOrder = 0 };
        table.Columns.Add(col);
        var row = new Row { RowOrder = 0 };
        table.Rows.Add(row);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        var act = () => _sut.UpsertCellAsync(row.RowId, col.ColumnId, CustomDataType.Int, "not-a-number");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpsertCellAsync_WhenChangingDataType_ClearsOtherValueFields()
    {
        // Arrange
        var table = new Table { Name = "Table" };
        var col = new Column { Name = "Col", DataType = CustomDataType.Int, ColOrder = 0 };
        table.Columns.Add(col);
        var row = new Row { RowOrder = 0 };
        table.Rows.Add(row);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.UpsertCellAsync(row.RowId, col.ColumnId, CustomDataType.Int, "10");

        await _sut.UpsertCellAsync(row.RowId, col.ColumnId, CustomDataType.String, "Now a string");

        // Assert
        var dbCell = await Context.CustomCells.FindAsync(row.RowId, col.ColumnId);
        dbCell!.ValInt.Should().BeNull();
        dbCell.ValString.Should().Be("Now a string");
    }

    // TableExistsAsync

    [Fact]
    public async Task TableExistsAsync_WhenTableExists_ReturnsTrueAndFalseWhenMissing()
    {
        // Arrange
        var table = new Table { Name = "Table" };
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        var exists = await _sut.TableExistsAsync(table.TableId);
        var missing = await _sut.TableExistsAsync(999);

        // Assert
        exists.Should().BeTrue();
        missing.Should().BeFalse();
    }

    // GetExistingRowIdsAsync

    [Fact]
    public async Task GetExistingRowIdsAsync_WhenRowIdsProvided_ReturnsOnlyExistingRows()
    {
        // Arrange
        var t1 = new Table { Name = "T1" };
        var r1 = new Row { RowOrder = 0 };
        t1.Rows.Add(r1);
        var t2 = new Table { Name = "T2" };
        var r2 = new Row { RowOrder = 0 };
        t2.Rows.Add(r2);
        Context.CustomTables.AddRange(t1, t2);
        await Context.SaveChangesAsync();

        var rowIds = new List<long> { r1.RowId, r2.RowId, 999 };

        // Act
        var result = await _sut.GetExistingRowIdsAsync(t1.TableId, rowIds);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(r1.RowId);
    }
}
