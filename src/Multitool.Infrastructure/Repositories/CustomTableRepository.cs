using System.Data;
using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.CustomTable;
using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;


namespace Multitool.Infrastructure.Repositories;

public class CustomTableRepository(AppDbContext db) : ICustomTableRepository
{
    public async Task<List<Table>> GetTableListAsync()
    {
        return await db.CustomTables
            .OrderBy(t => t.Name)
            .Select(t => new Table { TableId = t.TableId, Name = t.Name })
            .ToListAsync();
    }

    public async Task<Table> GetTableAsync(long tableId)
    {
        var table = await db.CustomTables
            .Where(t => t.TableId == tableId)
            .Select(t => new Table
            {
                TableId  = t.TableId,
                Name     = t.Name,
                CreatedAt = t.CreatedAt,
                Columns  = t.Columns
                    .OrderBy(c => c.ColOrder)
                    .Select(c => new Column
                    {
                        ColumnId  = c.ColumnId,
                        Name      = c.Name,
                        DataType  = c.DataType,
                        ColOrder  = c.ColOrder
                    })
                    .ToList(),
                Rows = t.Rows
                    .OrderBy(r => r.RowId)
                    .Select(r => new Row
                    {
                        RowId    = r.RowId,
                        RowOrder = r.RowOrder,
                        Cells    = r.Cells.Select(c => new Cell
                        {
                            ColumnId  = c.ColumnId,
                            ValString = c.ValString,
                            ValInt    = c.ValInt,
                            ValDec    = c.ValDec,
                            ValDate   = c.ValDate,
                            ValBool   = c.ValBool
                        }).ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (table is null)
            throw new NotFoundException($"Table with Id {tableId} not found");

        return table;
    }

    public async Task<long> CreateTableAsync(Table table, Column column)
    {
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync();

            db.CustomTables.Add(table);
            db.CustomColumns.Add(column);
            await db.SaveChangesAsync();

            await tx.CommitAsync();

            return table.TableId;
        });
    }

    public async Task UpdateTableAsync(long tableId, string newName)
    {
        var table = await db.CustomTables.FindAsync(new object?[] { tableId });

        if (table is null)
            throw new NotFoundException($"Table with Id {tableId} not found");

        table.Name = newName;
        await db.SaveChangesAsync();
    }

    public async Task DeleteTableAsync(long tableId)
    {
        var table = await db.CustomTables.FindAsync(new object?[] { tableId });

        if (table is null)
            throw new NotFoundException($"Table with Id {tableId} not found");

        db.CustomTables.Remove(table);
        await db.SaveChangesAsync();
    }

    public async Task CreateColumnAsync(long tableId)
    {
        var tableExists = await db.CustomTables.AnyAsync(t => t.TableId == tableId);

        if (!tableExists)
            throw new NotFoundException($"Table with Id {tableId} not found");

        var maxOrder = await db.CustomColumns
            .Where(c => c.TableId == tableId)
            .Select(c => (int?)c.ColOrder)
            .MaxAsync() ?? -1;

        var column = new Column
        {
            TableId  = tableId,
            Name     = "Neue Spalte",
            DataType = CustomDataType.String,
            ColOrder = maxOrder + 1
        };

        await db.CustomColumns.AddAsync(column);
        await db.SaveChangesAsync();
    }

    public async Task UpdateColumnAsync(Column column)
    {
        var col = await db.CustomColumns
            .FirstOrDefaultAsync(c => c.ColumnId == column.ColumnId);

        if (col is null)
            throw new NotFoundException($"Column with Id {column.ColumnId} not found");

        var typeChanged = col.DataType != column.DataType;

        col.Name     = column.Name;
        col.ColOrder = column.ColOrder;

        if (typeChanged)
        {
            col.DataType = column.DataType;

            await db.CustomCells
                .Where(c => c.ColumnId == column.ColumnId)
                .ExecuteDeleteAsync();
        }

        await db.SaveChangesAsync();
    }

    public async Task UpdateColumnOrderAsync(List<Column> columns)
    {
        var ids = columns.Select(c => c.ColumnId).ToList();

        var oldColumns = await db.CustomColumns
            .Where(c => ids.Contains(c.ColumnId))
            .ToListAsync();

        foreach (var col in oldColumns)
        {
            var update = columns.FirstOrDefault(c => c.ColumnId == col.ColumnId);
            if (update is not null)
                col.ColOrder = update.ColOrder;
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteColumnAsync(long tableId, long columnId)
    {
        var column = await db.CustomColumns
            .FirstOrDefaultAsync(c => c.ColumnId == columnId && c.TableId == tableId);

        if (column is null)
            throw new NotFoundException($"Column with Id {columnId} not found in table {tableId}");

        db.CustomColumns.Remove(column);
        await db.SaveChangesAsync();
    }

    public async Task CreateRowAsync(long tableId)
    {
        var tableExists = await db.CustomTables.AnyAsync(t => t.TableId == tableId);

        if (!tableExists)
            throw new NotFoundException($"Table with Id {tableId} not found");

        var maxOrder = await db.CustomRows
            .Where(r => r.TableId == tableId)
            .Select(r => (int?)r.RowOrder)
            .MaxAsync() ?? -1;

        var row = new Row
        {
            TableId   = tableId,
            CreatedAt = DateTime.UtcNow,
            RowOrder  = maxOrder + 1
        };

        await db.CustomRows.AddAsync(row);
        await db.SaveChangesAsync();
    }

    public async Task UpdateRowOrderAsync(List<Row> list)
    {
        var ids = list.Select(r => r.RowId).ToList();

        var rows = await db.CustomRows
            .Where(r => ids.Contains(r.RowId))
            .ToListAsync();

        foreach (var item in list)
        {
            var row = rows.FirstOrDefault(r => r.RowId == item.RowId)
                ?? throw new NotFoundException($"Row with Id {item.RowId} not found");

            row.RowOrder = item.RowOrder;
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteRowsAsync(long tableId, List<long> rowIds)
    {
        var existingIds = await db.CustomRows
            .Where(r => r.TableId == tableId && rowIds.Contains(r.RowId))
            .Select(r => r.RowId)
            .ToListAsync();

        var missing = rowIds.Except(existingIds).ToList();

        if (missing.Count > 0)
            throw new NotFoundException($"Rows not found in table {tableId}: {string.Join(", ", missing)}");

        await db.CustomRows
            .Where(r => r.TableId == tableId && rowIds.Contains(r.RowId))
            .ExecuteDeleteAsync();
    }

    public async Task UpsertCellAsync(long rowId, long columnId, object? value)
    {
        var rowExists = await db.CustomRows.AnyAsync(r => r.RowId == rowId);

        if (!rowExists)
            throw new NotFoundException($"Row with Id {rowId} not found");

        var column = await db.CustomColumns
            .Where(c => c.ColumnId == columnId)
            .Select(c => new { c.TableId, c.DataType })
            .FirstOrDefaultAsync();

        if (column is null)
            throw new NotFoundException($"Column with Id {columnId} not found");

        var cell = await db.CustomCells.FindAsync(new object?[] { rowId, columnId });

        if (cell is null)
        {
            cell = new Cell { RowId = rowId, ColumnId = columnId };
            db.CustomCells.Add(cell);
        }

        cell.ValString = null;
        cell.ValInt    = null;
        cell.ValDec    = null;
        cell.ValDate   = null;
        cell.ValBool   = null;

        switch (column.DataType)
        {
            case CustomDataType.String:
                cell.ValString = value?.ToString();
                break;

            case CustomDataType.Int when int.TryParse(value?.ToString(), out var i):
                cell.ValInt = i;
                break;

            case CustomDataType.Decimal when decimal.TryParse(value?.ToString(), out var d):
                cell.ValDec = d;
                break;

            case CustomDataType.Date when DateTime.TryParse(value?.ToString(), out var dt):
                cell.ValDate = dt;
                break;

            case CustomDataType.Bool when bool.TryParse(value?.ToString(), out var b):
                cell.ValBool = b;
                break;

            default:
                throw new ArgumentException($"Unsupported data type or invalid value for type {column.DataType}: '{value}'");
        }

        await db.SaveChangesAsync();
    }
}
