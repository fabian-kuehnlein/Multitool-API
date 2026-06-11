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

    [Fact]
    public async Task CreateTableAsync_AddsTableToDatabase()
    {
        var table = new Table { Name = "Test Table" };
        
        var id = await _sut.CreateTableAsync(table);

        id.Should().BeGreaterThan(0);
        var dbTable = await Context.CustomTables.FindAsync(id);
        dbTable.Should().NotBeNull();
        dbTable!.Name.Should().Be("Test Table");
    }

    [Fact]
    public async Task GetTableAsync_ReturnsTableWithColumnsAndRows()
    {
        var table = new Table { Name = "Complex Table" };
        table.Columns.Add(new Column { Name = "Col 1", DataType = CustomDataType.String, ColOrder = 0 });
        table.Rows.Add(new Row { RowOrder = 0 });
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        var result = await _sut.GetTableAsync(table.TableId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Complex Table");
        result.Columns.Should().HaveCount(1);
        result.Rows.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateColumnAsync_WhenTypeChanged_DeletesCells()
    {
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
        await _sut.UpdateColumnAsync(col, true);

        var dbCells = await Context.CustomCells.Where(c => c.ColumnId == col.ColumnId).ToListAsync();
        dbCells.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTableListAsync_ReturnsAllTablesSortedByName()
    {
        Context.CustomTables.AddRange(
            new Table { Name = "B" },
            new Table { Name = "A" }
        );
        await Context.SaveChangesAsync();

        var result = await _sut.GetTableListAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("A");
        result[1].Name.Should().Be("B");
    }

    [Fact]
    public async Task UpdateTableAsync_UpdatesName()
    {
        var table = new Table { Name = "Old" };
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        table.Name = "New";
        await _sut.UpdateTableAsync(table);

        var dbTable = await Context.CustomTables.FindAsync(table.TableId);
        dbTable!.Name.Should().Be("New");
    }

    [Fact]
    public async Task DeleteTableAsync_RemovesTableAndRelatedData()
    {
        var table = new Table { Name = "To Delete" };
        var col = new Column { Name = "Col", DataType = CustomDataType.String, ColOrder = 0 };
        table.Columns.Add(col);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        await _sut.DeleteTableAsync(table.TableId);

        var dbTable = await Context.CustomTables.FindAsync(table.TableId);
        dbTable.Should().BeNull();
        var dbCol = await Context.CustomColumns.FindAsync(col.ColumnId);
        dbCol.Should().BeNull();
    }

    [Fact]
    public async Task CreateColumnAsync_AddsColumnWithNextOrder()
    {
        var table = new Table { Name = "Table" };
        table.Columns.Add(new Column { Name = "Col 0", DataType = CustomDataType.String, ColOrder = 0 });
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        await _sut.CreateColumnAsync(table.TableId);

        var dbCols = await Context.CustomColumns.Where(c => c.TableId == table.TableId).ToListAsync();
        dbCols.Should().HaveCount(2);
        dbCols.Should().Contain(c => c.Name == "Neue Spalte" && c.ColOrder == 1);
    }

    [Fact]
    public async Task UpdateColumnOrderAsync_UpdatesOrders()
    {
        var table = new Table { Name = "Table" };
        var c1 = new Column { Name = "C1", DataType = CustomDataType.String, ColOrder = 0 };
        var c2 = new Column { Name = "C2", DataType = CustomDataType.String, ColOrder = 1 };
        table.Columns.AddRange(c1, c2);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        c1.ColOrder = 1;
        c2.ColOrder = 0;
        await _sut.UpdateColumnOrderAsync(new List<Column> { c1, c2 });

        var dbC1 = await Context.CustomColumns.FindAsync(c1.ColumnId);
        var dbC2 = await Context.CustomColumns.FindAsync(c2.ColumnId);
        dbC1!.ColOrder.Should().Be(1);
        dbC2!.ColOrder.Should().Be(0);
    }

    [Fact]
    public async Task CreateRowAsync_AddsRowWithNextOrder()
    {
        var table = new Table { Name = "Table" };
        table.Rows.Add(new Row { RowOrder = 0 });
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        await _sut.CreateRowAsync(table.TableId);

        var dbRows = await Context.CustomRows.Where(r => r.TableId == table.TableId).ToListAsync();
        dbRows.Should().HaveCount(2);
        dbRows.Should().Contain(r => r.RowOrder == 1);
    }

    [Fact]
    public async Task UpdateRowOrderAsync_UpdatesOrders()
    {
        var table = new Table { Name = "Table" };
        var r1 = new Row { RowOrder = 0 };
        var r2 = new Row { RowOrder = 1 };
        table.Rows.AddRange(r1, r2);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        await _sut.UpdateRowOrderAsync(new Dictionary<long, int> 
        { 
            { r1.RowId, 1 }, 
            { r2.RowId, 0 } 
        });

        var dbR1 = await Context.CustomRows.FindAsync(r1.RowId);
        var dbR2 = await Context.CustomRows.FindAsync(r2.RowId);
        dbR1!.RowOrder.Should().Be(1);
        dbR2!.RowOrder.Should().Be(0);
    }

    [Fact]
    public async Task DeleteRowsAsync_RemovesSpecificRows()
    {
        var table = new Table { Name = "Table" };
        var r1 = new Row { RowOrder = 0 };
        var r2 = new Row { RowOrder = 1 };
        table.Rows.AddRange(r1, r2);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        await _sut.DeleteRowsAsync(table.TableId, new List<long> { r1.RowId });

        var dbRows = await Context.CustomRows.Where(r => r.TableId == table.TableId).ToListAsync();
        dbRows.Should().HaveCount(1);
        dbRows[0].RowId.Should().Be(r2.RowId);
    }

    [Fact]
    public async Task TableExistsAsync_ReturnsCorrectResult()
    {
        var table = new Table { Name = "Table" };
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        (await _sut.TableExistsAsync(table.TableId)).Should().BeTrue();
        (await _sut.TableExistsAsync(999)).Should().BeFalse();
    }

    [Fact]
    public async Task UpsertCellAsync_UpdatesExistingCell()
    {
        var table = new Table { Name = "Table" };
        var col = new Column { Name = "Col", DataType = CustomDataType.String, ColOrder = 0 };
        table.Columns.Add(col);
        var row = new Row { RowOrder = 0 };
        table.Rows.Add(row);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();

        await _sut.UpsertCellAsync(row.RowId, col.ColumnId, CustomDataType.String, "New Value");

        var dbCell = await Context.CustomCells.FindAsync(row.RowId, col.ColumnId);
        dbCell.Should().NotBeNull();
        dbCell!.ValString.Should().Be("New Value");
    }

    [Fact]
    public async Task DeleteColumnAsync_RemovesColumn()
    {
        var table = new Table { Name = "Table" };
        var col = new Column { Name = "Col", DataType = CustomDataType.String, ColOrder = 0 };
        table.Columns.Add(col);
        Context.CustomTables.Add(table);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        await _sut.DeleteColumnAsync(col.ColumnId);

        var dbCol = await Context.CustomColumns.FindAsync(col.ColumnId);
        dbCol.Should().BeNull();
    }

    [Fact]
    public async Task GetExistingRowIdsAsync_ReturnsOnlyRowsThatExistInTable()
    {
        var t1 = new Table { Name = "T1" };
        var r1 = new Row { RowOrder = 0 };
        t1.Rows.Add(r1);
        var t2 = new Table { Name = "T2" };
        var r2 = new Row { RowOrder = 0 };
        t2.Rows.Add(r2);
        Context.CustomTables.AddRange(t1, t2);
        await Context.SaveChangesAsync();

        var result = await _sut.GetExistingRowIdsAsync(t1.TableId, new List<long> { r1.RowId, r2.RowId, 999 });

        result.Should().HaveCount(1);
        result.Should().Contain(r1.RowId);
    }
}
