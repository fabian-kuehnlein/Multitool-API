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
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new Table { TableId = t.TableId, Name = t.Name })
            .ToListAsync();
    }

    public async Task<Table?> GetTableAsync(long tableId)
    {
        return await db.CustomTables
            .AsNoTracking()
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
    }

    public async Task<long> CreateTableAsync(Table table)
    {
        db.CustomTables.Add(table);
        await db.SaveChangesAsync();

        return table.TableId;
    }

    public async Task UpdateTableAsync(Table table)
    {
        db.CustomTables.Update(table);
        await db.SaveChangesAsync();
    }

    public async Task DeleteTableAsync(long tableId)
    {
        await db.CustomTables
            .Where(t => t.TableId == tableId)
            .ExecuteDeleteAsync();
    }

    public async Task CreateColumnAsync(long tableId)
    {
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

    public async Task UpdateColumnAsync(Column col, bool typeChanged)
    {
        if (typeChanged)
        {
            await db.CustomCells
                .Where(c => c.ColumnId == col.ColumnId)
                .ExecuteDeleteAsync();
        }

        db.CustomColumns.Update(col);
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

    public async Task DeleteColumnAsync(long columnId)
    {
        await db.CustomColumns
            .Where(c => c.ColumnId == columnId)
            .ExecuteDeleteAsync();
    }

    public async Task CreateRowAsync(long tableId)
    {
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

    public async Task UpdateRowOrderAsync(List<RowOrderUpdateDto> list)
    {
        var ids = list.Select(r => r.RowId).ToList();

        var rows = await db.CustomRows
            .Where(r => ids.Contains(r.RowId))
            .ToListAsync();

        foreach (var item in list)
        {
            var row = rows.FirstOrDefault(r => r.RowId == item.RowId);
            if (row != null)
                row.RowOrder = item.RowOrder;
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteRowsAsync(long tableId, List<long> rowIds)
    {
        await db.CustomRows
            .Where(r => r.TableId == tableId && rowIds.Contains(r.RowId))
            .ExecuteDeleteAsync();
    }

    public async Task UpsertCellAsync(long rowId, long columnId, CustomDataType dataType, object? value)
    {
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

        switch (dataType)
        {
            case CustomDataType.String:
                cell.ValString = value?.ToString();
                break;

            case CustomDataType.Int when long.TryParse(value?.ToString(), out var i):
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
                throw new ArgumentException($"Unsupported data type or invalid value for type {dataType}: '{value}'");
        }

        await db.SaveChangesAsync();
    }

    public async Task<Table?> GetTableRawAsync(long tableId)
        => await db.CustomTables.FindAsync(tableId);

    public async Task<Column?> GetColumnAsync(long columnId)
        => await db.CustomColumns.FindAsync(columnId);

    public async Task<Row?> GetRowAsync(long rowId)
        => await db.CustomRows.FindAsync(rowId);
    
    public async Task<bool> TableExistsAsync(long tableId)
        => await db.CustomTables.AnyAsync(t => t.TableId == tableId);
    
    public async Task<List<long>> GetExistingRowIdsAsync(long tableId, List<long> rowIds)
        => await db.CustomRows
            .Where(r => r.TableId == tableId && rowIds.Contains(r.RowId))
            .Select(r => r.RowId)
            .ToListAsync();

}
