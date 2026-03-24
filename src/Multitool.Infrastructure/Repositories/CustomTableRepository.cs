using MultitoolApi.WebApi.Models.CustomTable;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;
using Multitool.Domain.Entities.CustomTable;
using Multitool.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Multitool.Infrastructure.Repositories;

public class CustomTableRepository(AppDbContext db, ILogger<CalendarRepository> logger) : ICustomTableRepository
{
    public async Task<List<TableOverview>> GetTableListAsync()
    {
        return await db.CustomTables
            .OrderBy(t => t.Name)
            .Select(t => new TableOverview(t.TableId, t.Name))
            .ToListAsync();
    }

    public async Task<TableDetail?> GetTableAsync(long tableId)
    {
        const int take = 100;

        // Spalten: unverändert (kein client‑only Code)
        var columns = await db.CustomColumns
            .Where(c => c.TableId == tableId)
            .OrderBy(c => c.ColOrder)
            .Select(c => new ColumnInfo(c.ColumnId, c.Name, c.DataType, c.ColOrder))
            .ToListAsync();

        // Zeilen: 2‑Phasen‑Ansatz
        var flatRows = await db.CustomRows
            .Where(r => r.TableId == tableId)
            .OrderBy(r => r.RowId)
            .Take(take)
            .Select(r => new
            {
                r.RowId,
                r.RowOrder,
                Cells = r.Cells.Select(c => new
                {
                    c.ColumnId,
                    c.ValString,
                    c.ValInt,
                    c.ValDec,
                    c.ValDate,
                    c.ValBool
                }).ToList()
            })
            .ToListAsync();

        var rows = flatRows.Select(r => new RowInfo(
                r.RowId,
                r.Cells.ToDictionary(
                    c => c.ColumnId,
                    c => (object?)(
                        c.ValString
                        ?? (object?)c.ValInt
                        ?? c.ValDec
                        ?? (object?)c.ValDate
                        ?? c.ValBool)),
                r.RowOrder
            )).ToList();

        var t = await db.CustomTables
            .FirstOrDefaultAsync(tt => tt.TableId == tableId);

        return t is null
            ? null
            : new TableDetail(t.TableId, t.Name, t.CreatedAt, columns, rows);
    }

    public async Task<long> CreateTableAsync(CreateTableDto dto)
    {
        var stradegy = db.Database.CreateExecutionStrategy();

        return await stradegy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync();

            var table = new CustomTable
            {
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow
            };

            db.CustomTables.Add(table);
            await db.SaveChangesAsync();

            var column = new CustomColumn
            {
                TableId = table.TableId,
                Name = dto.Column.Name,
                DataType = dto.Column.DataType,
                ColOrder = 0
            };

            db.CustomColumns.Add(column);
            await db.SaveChangesAsync();

            await tx.CommitAsync();

            return table.TableId;
        });
    }

    public async Task UpdateTableAsync(long tableId, string newName)
    {
        var table = await db.CustomTables.FindAsync(new object?[] { tableId });

        if (table is not null)
        {
            table.Name = newName;
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteTableAsync(long tableId)
    {
        var table = await db.CustomTables.FindAsync(new object?[] { tableId });

        if (table is not null)
        {
            db.CustomTables.Remove(table);
            await db.SaveChangesAsync();
        }
    }

    public async Task CreateColumnAsync(long tableId)
    {
        var maxOrder = await db.CustomColumns
            .Where(c => c.TableId == tableId)
            .Select(c => (int?)c.ColOrder)
            .MaxAsync() ?? -1;

        var column = new CustomColumn
        {
            TableId = tableId,
            Name = "Neue Spalte",
            DataType = CustomDataType.String,
            ColOrder = maxOrder + 1
        };

        await db.CustomColumns.AddAsync(column);
        await db.SaveChangesAsync();
    }

    public async Task UpdateColumnAsync(long columnId, UpdateColumnDto dto)
    {
        var column = await db.CustomColumns
            .FirstOrDefaultAsync(c => c.ColumnId == columnId);

        if (column is null) throw new KeyNotFoundException("Column not found");

        var typeChanged = column.DataType != dto.DataType;

        column.Name = dto.Name;
        column.ColOrder = dto.ColOrder;

        if (typeChanged)
        {
            column.DataType = dto.DataType;

            await db.CustomCells
                .Where(c => c.ColumnId == columnId)
                .ExecuteDeleteAsync();
        }

        await db.SaveChangesAsync();
    }

    public async Task UpdateColumnOrderAsync(List<UpdateColumnOrderDto> cols)
    {
        var ids = cols.Select(c => c.ColumnId).ToList();

        var columns = await db.CustomColumns
            .Where(c => ids.Contains(c.ColumnId))
            .ToListAsync();

        foreach (var col in columns)
        {
            var update = cols.FirstOrDefault(c => c.ColumnId == col.ColumnId);
            if (update != null)
            {
                col.ColOrder = update.ColOrder;
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteColumnAsync(long tableId, long columnId)
    {
        var column = await db.CustomColumns
            .FirstOrDefaultAsync(c => c.ColumnId == columnId && c.TableId == tableId);

        if (column is null) return;

        db.CustomColumns.Remove(column);
        await db.SaveChangesAsync();
    }

    public async Task CreateRowAsync(long tableId)
    {
        var maxOrder = await db.CustomRows
            .Where(r => r.TableId == tableId)
            .Select(r => (int?)r.RowOrder)
            .MaxAsync() ?? -1;

        var row = new CustomRow
        {
            TableId = tableId,
            CreatedAt = DateTime.UtcNow,
            RowOrder = maxOrder + 1
        };

        await db.CustomRows.AddAsync(row);
        await db.SaveChangesAsync();
    }

    public async Task UpdateRowOrderAsync(List<RowOrderUpdateDto> list)
    {
        foreach (var item in list)
        {
            var row = await db.CustomRows.FirstOrDefaultAsync(r => r.RowId == item.RowId);

            if (row != null)
            {
                row.RowOrder = item.RowOrder;
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteRowsAsync(long tableId, List<long> rows)
    {
        await db.CustomRows
            .Where(r => r.TableId == tableId && rows.Contains(r.RowId))
            .ExecuteDeleteAsync();
    }

    public async Task UpsertCellAsync(long rowId, long columnId, object? value)
    {
        var column = await db.CustomColumns
            .Where(c => c.ColumnId == columnId)
            .Select(c => new { c.TableId, c.DataType })
            .FirstOrDefaultAsync();

        if (column == null)
        {
            throw new ArgumentException($"Column {columnId} not found");
        }

        var cell = await db.CustomCells
            .FindAsync(new object?[] { rowId, columnId });

        // create cell if it does not exist
        if (cell == null)
        {
            cell = new CustomCell { RowId = rowId, ColumnId = columnId };
            db.CustomCells.Add(cell);
        }

        cell.ValString = null;
        cell.ValInt = null;
        cell.ValDec = null;
        cell.ValDate = null;
        cell.ValBool = null;

        switch (column.DataType)
        {
            case CustomDataType.String:
                cell.ValString = value!.ToString();
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
                throw new ArgumentException($"Unsupported data type: {column.DataType}");
        }

        await db.SaveChangesAsync();
    }
}
