using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.CustomTable;
using Multitool.Domain.Enums;
using Multitool.Infrastructure.Repositories;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Infrastructure.Tests;

public class CustomTableRepositoryTests : RepositoryTestBase
{
    private readonly CustomTableRepository _sut;

    public CustomTableRepositoryTests()
    {
        _sut = new CustomTableRepository(Context);
    }

    private static Table CreateTable(string name)
    {
        var table = CustomTableTestData.DefaultTable;
        table.TableId = 0;
        table.Name = name;
        return table;
    }

    private static Column CreateColumn(string name, int colOrder, CustomDataType dataType = CustomDataType.String)
    {
        var column = CustomTableTestData.DefaultColumn;
        column.ColumnId = 0;
        column.TableId = 0;
        column.Name = name;
        column.ColOrder = colOrder;
        column.DataType = dataType;
        return column;
    }

    private static Row CreateRow(int rowOrder)
    {
        var row = CustomTableTestData.DefaultRow;
        row.RowId = 0;
        row.TableId = 0;
        row.RowOrder = rowOrder;
        return row;
    }

    // GetTableListAsync

    [Fact]
    public async Task GetTableListAsync_WhenTablesExist_ReturnsAllTablesSortedByName()
    {
        // Arrange
        Context.CustomTables.AddRange(
            CreateTable("B"),
            CreateTable("A")
        );
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetTableListAsync();

        // Assert
        AssertEx.AreEqual(2, result.Count);
        AssertEx.AreEqual("A", result[0].Name);
        AssertEx.AreEqual("B", result[1].Name);
    }

    // GetTableAsync

    [Fact]
    public async Task GetTableAsync_WhenTableExists_ReturnsTableWithColumnsAndRows()
    {
        // Arrange
        var table = CustomTableTestData.DefaultTable;
        table.Columns.Add(CreateColumn("Col 1", 0));
        table.Rows.Add(CreateRow(0));
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetTableAsync(table.TableId);

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(table.Name, result!.Name);
        AssertEx.AreEqual(1, result.Columns.Count);
        AssertEx.AreEqual(1, result.Rows.Count);
    }

    [Fact]
    public async Task GetTableAsync_WhenTableDoesNotExist_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _sut.GetTableAsync(999);

        // Assert
        Assert.Null(result);
    }

    // CreateTableAsync

    [Fact]
    public async Task CreateTableAsync_WhenTableIsValid_AddsTableToDatabase()
    {
        // Arrange
        var table = CreateTable("Test Table");

        // Act
        var id = await _sut.CreateTableAsync(table);

        // Assert
        Assert.True(id > 0);
        var dbTable = await Context.CustomTables.AsNoTracking().FirstOrDefaultAsync(t => t.TableId == id);
        Assert.NotNull(dbTable);
        AssertEx.AreEqual("Test Table", dbTable!.Name);
    }

    // UpdateTableAsync

    [Fact]
    public async Task UpdateTableAsync_WhenTableExists_UpdatesName()
    {
        // Arrange
        var table = CreateTable("Old");
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        table.Name = "New";

        // Act
        await _sut.UpdateTableAsync(table);

        // Assert
        var dbTable = await Context.CustomTables.AsNoTracking().FirstOrDefaultAsync(t => t.TableId == table.TableId);
        Assert.NotNull(dbTable);
        AssertEx.AreEqual("New", dbTable!.Name);
    }

    // DeleteTableAsync

    [Fact]
    public async Task DeleteTableAsync_WhenTableExists_RemovesTableAndRelatedData()
    {
        // Arrange
        var table = CreateTable("To Delete");
        var col = CreateColumn("Col", 0);
        table.Columns.Add(col);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteTableAsync(table.TableId);

        // Assert
        var dbTable = await Context.CustomTables.AsNoTracking().FirstOrDefaultAsync(t => t.TableId == table.TableId);
        Assert.Null(dbTable);
        var dbCol = await Context.CustomColumns.AsNoTracking().FirstOrDefaultAsync(c => c.ColumnId == col.ColumnId);
        Assert.Null(dbCol);
    }

    // CreateColumnAsync

    [Fact]
    public async Task CreateColumnAsync_WhenColumnsExist_AddsColumnWithNextOrder()
    {
        // Arrange
        var table = CreateTable("Table");
        table.Columns.Add(CreateColumn("Col 0", 0));
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.CreateColumnAsync(table.TableId);

        // Assert
        var dbCols = await Context.CustomColumns.Where(c => c.TableId == table.TableId).ToListAsync();
        AssertEx.AreEqual(2, dbCols.Count);
        Assert.Contains(dbCols, c => c.Name == "Neue Spalte" && c.ColOrder == 1);
    }

    // UpdateColumnAsync

    [Fact]
    public async Task UpdateColumnAsync_WhenTypeChanged_DeletesCells()
    {
        // Arrange
        var table = CreateTable("Table");
        var col = CreateColumn("Col", 0);
        table.Columns.Add(col);
        table.Rows.Add(CreateRow(0));
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        var cell = new Cell { RowId = table.Rows[0].RowId, ColumnId = col.ColumnId, ValString = "Data" };
        Context.CustomCells.Add(cell);
        await Context.SaveChangesAsync();

        col.DataType = CustomDataType.Int;

        // Act
        await _sut.UpdateColumnAsync(col, true);

        // Assert
        var dbCells = await Context.CustomCells.Where(c => c.ColumnId == col.ColumnId).ToListAsync();
        Assert.Empty(dbCells);
    }

    // UpdateColumnOrderAsync

    [Fact]
    public async Task UpdateColumnOrderAsync_WhenColumnsExist_UpdatesOrders()
    {
        // Arrange
        var table = CreateTable("Table");
        var c1 = CreateColumn("C1", 0);
        var c2 = CreateColumn("C2", 1);
        table.Columns.AddRange(c1, c2);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        c1.ColOrder = 1;
        c2.ColOrder = 0;

        // Act
        await _sut.UpdateColumnOrderAsync(new List<Column> { c1, c2 });

        // Assert
        var dbC1 = await Context.CustomColumns.AsNoTracking().FirstOrDefaultAsync(c => c.ColumnId == c1.ColumnId);
        var dbC2 = await Context.CustomColumns.AsNoTracking().FirstOrDefaultAsync(c => c.ColumnId == c2.ColumnId);
        Assert.NotNull(dbC1);
        Assert.NotNull(dbC2);
        AssertEx.AreEqual(1, dbC1!.ColOrder);
        AssertEx.AreEqual(0, dbC2!.ColOrder);
    }

    // DeleteColumnAsync

    [Fact]
    public async Task DeleteColumnAsync_WhenColumnExists_RemovesColumn()
    {
        // Arrange
        var table = CreateTable("Table");
        var col = CreateColumn("Col", 0);
        table.Columns.Add(col);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteColumnAsync(col.ColumnId);

        // Assert
        var dbCol = await Context.CustomColumns.AsNoTracking().FirstOrDefaultAsync(c => c.ColumnId == col.ColumnId);
        Assert.Null(dbCol);
    }

    // CreateRowAsync

    [Fact]
    public async Task CreateRowAsync_WhenRowsExist_AddsRowWithNextOrder()
    {
        // Arrange
        var table = CreateTable("Table");
        table.Rows.Add(CreateRow(0));
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.CreateRowAsync(table.TableId);

        // Assert
        var dbRows = await Context.CustomRows.Where(r => r.TableId == table.TableId).ToListAsync();
        AssertEx.AreEqual(2, dbRows.Count);
        Assert.Contains(dbRows, r => r.RowOrder == 1);
    }

    // UpdateRowOrderAsync

    [Fact]
    public async Task UpdateRowOrderAsync_WhenRowsExist_UpdatesOrders()
    {
        // Arrange
        var table = CreateTable("Table");
        var r1 = CreateRow(0);
        var r2 = CreateRow(1);
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
        var dbR1 = await Context.CustomRows.AsNoTracking().FirstOrDefaultAsync(r => r.RowId == r1.RowId);
        var dbR2 = await Context.CustomRows.AsNoTracking().FirstOrDefaultAsync(r => r.RowId == r2.RowId);
        Assert.NotNull(dbR1);
        Assert.NotNull(dbR2);
        AssertEx.AreEqual(1, dbR1!.RowOrder);
        AssertEx.AreEqual(0, dbR2!.RowOrder);
    }

    // DeleteRowsAsync

    [Fact]
    public async Task DeleteRowsAsync_WhenRowIdsProvided_RemovesSpecificRows()
    {
        // Arrange
        var table = CreateTable("Table");
        var r1 = CreateRow(0);
        var r2 = CreateRow(1);
        table.Rows.AddRange(r1, r2);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        var rowIds = new List<long> { r1.RowId };

        // Act
        await _sut.DeleteRowsAsync(table.TableId, rowIds);

        // Assert
        var dbRows = await Context.CustomRows.Where(r => r.TableId == table.TableId).ToListAsync();
        AssertEx.AreEqual(1, dbRows.Count);
        AssertEx.AreEqual(r2.RowId, dbRows[0].RowId);
    }

    // UpsertCellAsync

    [Fact]
    public async Task UpsertCellAsync_WhenCellExists_UpdatesValue()
    {
        // Arrange
        var table = CreateTable("Table");
        var col = CreateColumn("Col", 0);
        table.Columns.Add(col);
        table.Rows.Add(CreateRow(0));
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.UpsertCellAsync(table.Rows[0].RowId, col.ColumnId, CustomDataType.String, "New Value");

        // Assert
        var dbCell = await Context.CustomCells.FindAsync(table.Rows[0].RowId, col.ColumnId);
        Assert.NotNull(dbCell);
        AssertEx.AreEqual("New Value", dbCell!.ValString);
    }

    [Fact]
    public async Task UpsertCellAsync_WhenCellDoesNotExist_CreatesNewCell()
    {
        // Arrange
        var table = CreateTable("Table");
        var col = CreateColumn("Col", 0);
        table.Columns.Add(col);
        table.Rows.Add(CreateRow(0));
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.UpsertCellAsync(table.Rows[0].RowId, col.ColumnId, CustomDataType.String, "First Value");

        // Assert
        var dbCell = await Context.CustomCells.FindAsync(table.Rows[0].RowId, col.ColumnId);
        Assert.NotNull(dbCell);
        AssertEx.AreEqual("First Value", dbCell!.ValString);
    }

    [Theory]
    [InlineData(CustomDataType.Int, "42")]
    [InlineData(CustomDataType.Decimal, "3.14")]
    [InlineData(CustomDataType.Bool, "true")]
    public async Task UpsertCellAsync_WhenValueMatchesDataType_StoresParsedValue(CustomDataType dataType, string value)
    {
        // Arrange
        var table = CreateTable("Table");
        var col = CreateColumn("Col", 0, dataType);
        table.Columns.Add(col);
        table.Rows.Add(CreateRow(0));
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.UpsertCellAsync(table.Rows[0].RowId, col.ColumnId, dataType, value);

        // Assert
        var dbCell = await Context.CustomCells.FindAsync(table.Rows[0].RowId, col.ColumnId);
        Assert.NotNull(dbCell);

        switch (dataType)
        {
            case CustomDataType.Int:
                AssertEx.AreEqual(42L, dbCell!.ValInt);
                break;
            case CustomDataType.Decimal:
                AssertEx.AreEqual(3.14m, dbCell!.ValDec);
                break;
            case CustomDataType.Bool:
                Assert.True(dbCell!.ValBool);
                break;
        }
    }

    [Fact]
    public async Task UpsertCellAsync_WhenDateValueIsValid_StoresDate()
    {
        // Arrange
        var table = CreateTable("Table");
        var col = CreateColumn("Col", 0, CustomDataType.Date);
        table.Columns.Add(col);
        table.Rows.Add(CreateRow(0));
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.UpsertCellAsync(table.Rows[0].RowId, col.ColumnId, CustomDataType.Date, "2026-06-12");

        // Assert
        var dbCell = await Context.CustomCells.FindAsync(table.Rows[0].RowId, col.ColumnId);
        Assert.NotNull(dbCell);
        AssertEx.AreEqual((DateTime?)new DateTime(2026, 6, 12), dbCell!.ValDate);
    }

    [Fact]
    public async Task UpsertCellAsync_WhenValueDoesNotMatchDataType_StoresNoValue()
    {
        // Arrange
        var table = CreateTable("Table");
        var col = CreateColumn("Col", 0, CustomDataType.Int);
        table.Columns.Add(col);
        table.Rows.Add(CreateRow(0));
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.UpsertCellAsync(table.Rows[0].RowId, col.ColumnId, CustomDataType.Int, "not-a-number");

        // Assert
        var dbCell = await Context.CustomCells.FindAsync(table.Rows[0].RowId, col.ColumnId);
        Assert.NotNull(dbCell);
        Assert.Null(dbCell!.ValInt);
    }

    [Fact]
    public async Task UpsertCellAsync_WhenChangingDataType_ClearsOtherValueFields()
    {
        // Arrange
        var table = CreateTable("Table");
        var col = CreateColumn("Col", 0, CustomDataType.Int);
        table.Columns.Add(col);
        table.Rows.Add(CreateRow(0));
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        await _sut.UpsertCellAsync(table.Rows[0].RowId, col.ColumnId, CustomDataType.Int, "10");

        await _sut.UpsertCellAsync(table.Rows[0].RowId, col.ColumnId, CustomDataType.String, "Now a string");

        // Assert
        var dbCell = await Context.CustomCells.FindAsync(table.Rows[0].RowId, col.ColumnId);
        Assert.NotNull(dbCell);
        Assert.Null(dbCell!.ValInt);
        AssertEx.AreEqual("Now a string", dbCell.ValString);
    }

    // TableExistsAsync

    [Fact]
    public async Task TableExistsAsync_WhenTableExists_ReturnsTrueAndFalseWhenMissing()
    {
        // Arrange
        var table = CreateTable("Table");
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        // Act
        var exists = await _sut.TableExistsAsync(table.TableId);
        var missing = await _sut.TableExistsAsync(999);

        // Assert
        Assert.True(exists);
        Assert.False(missing);
    }

    // GetExistingRowIdsAsync

    [Fact]
    public async Task GetExistingRowIdsAsync_WhenRowIdsProvided_ReturnsOnlyExistingRows()
    {
        // Arrange
        var t1 = CreateTable("T1");
        t1.Rows.Add(CreateRow(0));
        var t2 = CreateTable("T2");
        var t2Row = CreateRow(0);
        t2.Rows.Add(t2Row);
        Context.CustomTables.AddRange(t1, t2);
        await Context.SaveChangesAsync();

        var rowIds = new List<long> { t1.Rows[0].RowId, t2Row.RowId, 999 };

        // Act
        var result = await _sut.GetExistingRowIdsAsync(t1.TableId, rowIds);

        // Assert
        AssertEx.AreEqual(1, result.Count);
        Assert.Contains(t1.Rows[0].RowId, result);
    }
}
