using MySqlConnector;
using MultitoolApi.DataAccessLayer.Models;
using MultitoolApi.Businesslogic.Models;
using System.Text.Json;
using MultitoolApi.WebApi.Models.CustomTable;
using System.Data;
using MultitoolApi.ConfigModels;
using Microsoft.EntityFrameworkCore;
using MultitoolApi.Infrastructure.DataAccessLayer.Models.CustomTable;
using Microsoft.AspNetCore.Http.Connections;

public class CustomTableRepository : ICustomTableRepository
{
    private readonly AppDbContext _db;
    private readonly string _connectionString;
    private readonly ILogger<CalendarEventRepository> _logger;

    public CustomTableRepository(AppDbContext db, IConfiguration configuration, ILogger<CalendarEventRepository> logger)
    {
        _db = db;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'MariaDBConnection' not found.");
        _logger = logger;
    }

    public async Task<List<TableOverview>> GetTableListAsync()
    {
        return await _db.CustomTables
            .OrderBy(t => t.Name)
            .Select(t => new TableOverview(t.TableId, t.Name))
            .ToListAsync();
    }

    public async Task<TableDetail?> GetTableAsync(long tableId)
    {
        const int take = 100;

        // Spalten: unverändert (kein client‑only Code)
        var columns = await _db.CustomColumns
            .Where(c => c.TableId == tableId)
            .OrderBy(c => c.ColOrder)
            .Select(c => new ColumnInfo(c.ColumnId, c.Name, c.DataType, c.ColOrder))
            .ToListAsync();

        // Zeilen: 2‑Phasen‑Ansatz
        var flatRows = await _db.CustomRows
            .Where(r => r.TableId == tableId)
            .OrderBy(r => r.RowId)
            .Take(take)
            .Select(r => new
            {
                r.RowId,
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
                        ?? c.ValBool)))
            ).ToList();

        var t = await _db.CustomTables
            .FirstOrDefaultAsync(tt => tt.TableId == tableId);

        return t is null
            ? null
            : new TableDetail(t.TableId, t.Name, t.CreatedAt, columns, rows);
    }

    public async Task<long> CreateTableAsync(CreateTableDto dto)
    {
        var stradegy = _db.Database.CreateExecutionStrategy();

        return await stradegy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var table = new CustomTable
            {
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow
            };

            _db.CustomTables.Add(table);
            await _db.SaveChangesAsync();

            var column = new CustomColumn
            {
                TableId = table.TableId,
                Name = dto.Column.Name,
                DataType = dto.Column.DataType,
                ColOrder = 0
            };

            _db.CustomColumns.Add(column);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            return table.TableId;
        });
    }

    public async Task UpdateTableAsync(long tableId, string newName)
    {
        var table = await _db.CustomTables.FindAsync(new object?[] { tableId });

        if (table is not null)
        {
            table.Name = newName;
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteTableAsync(long tableId)
    {
        var table = await _db.CustomTables.FindAsync(new object?[] { tableId });

        if (table is not null)
        {
            _db.CustomTables.Remove(table);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<ColumnInfo>> GetColumnsAsync(long tableId)
    {
        return await _db.CustomColumns
            .Where(c => c.TableId == tableId)
            .OrderBy(c => c.ColOrder)
            .Select(c => new ColumnInfo(c.ColumnId, c.Name, c.DataType, c.ColOrder))
            .ToListAsync();
    }

    public async Task CreateColumnAsync(long tableId)
    {
        var maxOrder = await _db.CustomColumns
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

        await _db.CustomColumns.AddAsync(column);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateColumnAsync(long tableId, long columnId, UpdateColumnDto dto)
    {
        var column = await _db.CustomColumns
            .FirstOrDefaultAsync(c => c.ColumnId == columnId && c.TableId == tableId);

        if (column is null) throw new KeyNotFoundException("Column not found");

        var typeChanged = column.DataType != dto.DataType;

        column.Name = dto.Name;
        column.ColOrder = dto.ColOrder;

        if (typeChanged)
        {
            column.DataType = dto.DataType;

            await _db.CustomCells
                .Where(c => c.ColumnId == columnId)
                .ExecuteDeleteAsync();
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteColumnAsync(long tableId, long columnId)
    {
        var column = await _db.CustomColumns
            .FirstOrDefaultAsync(c => c.ColumnId == columnId && c.TableId == tableId);

        if (column is null) return;

        _db.CustomColumns.Remove(column);
        await _db.SaveChangesAsync();
    }

    public async Task<List<RowInfo>> GetRowsAsync(long tableId, int pageNr, int pageSize)
    {
        var skip = (pageNr - 1) * pageSize;

        var rows = await _db.CustomRows
            .Where(r => r.TableId == tableId)
            .OrderBy(r => r.RowId)
            .Skip(skip)
            .Take(pageSize)
            .Select(r => new
            {
                r.RowId,
                Cells = r.Cells.Select(c => new
                {
                    c.ColumnId,
                    c.ValString,
                    c.ValInt,
                    c.ValDec,
                    c.ValDate,
                    c.ValBool
                }).ToList()
            }).ToListAsync();
        return rows.Select(r => new RowInfo(
            r.RowId,
            r.Cells.ToDictionary(
                c => c.ColumnId,
                c => (object?)(
                    c.ValString
                    ?? (object?)c.ValInt
                    ?? c.ValDec
                    ?? (object?)c.ValDate
                    ?? c.ValBool)))
            ).ToList();
    }

    public async Task CreateRowAsync(long tableId)
    {
        var row = new CustomRow
        {
            TableId = tableId,
            CreatedAt = DateTime.UtcNow
        };

        await _db.CustomRows.AddAsync(row);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateRowAsync(long tableId, long rowId, Dictionary<long, object?> cells)
    {
        var row = await _db.CustomRows
            .Include(r => r.Cells)
            .FirstOrDefaultAsync(r => r.RowId == rowId && r.TableId == tableId);

        if (row == null) return;

        // Update Cells
        foreach (var (columnId, newValue) in cells)
        {
            var cell = row.Cells.FirstOrDefault(c => c.ColumnId == columnId);
            if (cell == null)
            {
                // Neue Zelle anlegen, falls nicht vorhanden
                cell = new CustomCell
                {
                    RowId = row.RowId,
                    ColumnId = columnId
                };
                row.Cells.Add(cell);
            }

            cell.ValString = newValue as string;
            cell.ValInt = newValue is int i ? i : null;
            cell.ValDec = newValue is decimal d ? d : null;
            cell.ValDate = newValue is DateTime dt ? dt : null;
            cell.ValBool = newValue is bool b ? b : null;
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteRowAsync(long tableId, long rowId)
    {
        var row = await _db.CustomRows
            .FirstOrDefaultAsync(r => r.RowId == rowId && r.TableId == tableId);

        if (row == null) return;

        _db.CustomRows.Remove(row);
        await _db.SaveChangesAsync();
    }

    public async Task UpsertCellAsync(long rowId, long columnId, object? value)
    {
        var column = await _db.CustomColumns
            .Where(c => c.ColumnId == columnId)
            .Select(c => new { c.TableId, c.DataType })
            .FirstOrDefaultAsync();

        if (column == null)
        {
            throw new ArgumentException($"Column {columnId} not found");
        }

        var cell = await _db.CustomCells
            .FindAsync(new object?[] { rowId, columnId });

        // create cell if it does not exist
        if (cell == null)
        {
            cell = new CustomCell { RowId = rowId, ColumnId = columnId };
            _db.CustomCells.Add(cell);
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

        await _db.SaveChangesAsync();
    }
}
