using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Entities.Category;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Entities.CustomTable;
using Multitool.Domain.Entities.Todo;

namespace Multitool.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<CalendarEvent> CalendarEvents { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Todo> Todos { get; set; }
    public DbSet<Table> CustomTables { get; set; }
    public DbSet<Column> CustomColumns { get; set; }
    public DbSet<Row> CustomRows { get; set; }
    public DbSet<Cell> CustomCells { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultScheme("public");

        // ---------- CALENDAR ----------
        modelBuilder.Entity<CalendarEvent>(e =>
        {
            e.ToTable("calendar_events");
            e.HasKey(e => e.Id);

            e.Property(e => e.Id).HasColumnName("event_id").ValueGeneratedOnAdd();
            e.Property(e => e.Title).HasColumnName("event_title").IsRequired();
            e.Property(e => e.Note).HasColumnName("event_note");
            e.Property(e => e.StartDateTime).HasColumnName("start_date_time").IsRequired();
            e.Property(e => e.EndDateTime).HasColumnName("end_date_time");
            e.Property(e => e.IsAllDay).HasColumnName("is_all_day").IsRequired();
            e.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired();
            e.Property(e => e.RecurrenceRule).HasColumnName("recurrence_rule");
            e.Property(e => e.RecurrenceEnd).HasColumnName("recurrence_end");

            e.HasOne<Category>()
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .HasConstraintName("fk_calendar_events_category_id");
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.HasKey(c => c.Id);

            e.Property(c => c.Id).HasColumnName("category_id").ValueGeneratedOnAdd();
            e.Property(c => c.Name).HasColumnName("category_name").IsRequired();
            e.Property(c => c.Color).HasMaxLength(9).IsRequired();
        });

        // ---------- TODO ----------
        modelBuilder.Entity<Todo>(e =>
        {
            e.ToTable("todos");
            e.HasKey(e => e.Id);

            e.Property(e => e.Id).HasColumnName("todo_id").ValueGeneratedOnAdd();
            e.Property(e => e.Title).HasColumnName("todo_title").IsRequired();
            e.Property(e => e.Description).HasColumnName("todo_description");
            e.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired();
            e.Property(e => e.IsDone).HasColumnName("is_done").IsRequired();
            e.Property(e => e.Priority).HasColumnName("priority").IsRequired();
            e.Property(e => e.CreationDateTime).HasColumnName("creation_date_time").IsRequired();

            e.HasOne<Category>()
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .HasConstraintName("fk_todos_category_id");
        });

        // ---------- CUSTOM TABLE ----------
        modelBuilder.Entity<Table>(e =>
        {
            e.ToTable("tables", "custom");
            e.HasKey(t => t.TableId);

            e.Property(t => t.TableId).HasColumnName("table_id").ValueGeneratedOnAdd();
            e.Property(t => t.Name).HasColumnName("table_name").HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<Column>(e =>
        {
            e.ToTable("columns", "custom");
            e.HasKey(c => c.ColumnId);

            e.Property(c => c.ColumnId).HasColumnName("column_id").ValueGeneratedOnAdd();

            e.HasOne(c => c.Table)
             .WithMany(t => t.Columns)
             .HasForeignKey(c => c.TableId)
             .HasConstraintName("fk_custom_columns_table_id")
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(c => c.Name).HasMaxLength(120).IsRequired();
            e.Property(c => c.DataType).HasConversion<string>().IsRequired();
        });

        modelBuilder.Entity<Row>(e =>
        {
            e.ToTable("rows", "custom");
            e.HasKey(r => r.RowId);

            e.Property(r => r.RowId).HasColumnName("row_id");

            e.HasOne(r => r.Table)
             .WithMany(t => t.Rows)
             .HasForeignKey(r => r.TableId)
             .HasConstraintName("fk_custom_rows_table_id")
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cell>(e =>
        {
            e.ToTable("cells", "custom");
            e.HasKey(c => new { c.RowId, c.ColumnId });

            e.HasOne(c => c.Row)
             .WithMany(r => r.Cells)
             .HasForeignKey(c => c.RowId)
             .HasConstraintName("fk_custom_cells_row_id")
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.Column)
             .WithMany()
             .HasForeignKey(c => c.ColumnId)
             .HasConstraintName("fk_custom_cells_column_id")
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(c => new { c.ColumnId, c.ValInt }).HasDatabaseName("idx_cell_int");
            e.HasIndex(c => new { c.ColumnId, c.ValDec }).HasDatabaseName("idx_cell_dec");
            e.HasIndex(c => new { c.ColumnId, c.ValDate }).HasDatabaseName("idx_cell_date");
        });

        // ---------- GLOBAL SNAKE CASE ----------
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (tableName != null)
                entity.SetTableName(ToSnakeCase(tableName));

            foreach (var property in entity.GetProperties())
            {
                var explicitName = property.GetColumnName();
                if (explicitName == property.Name)
                    property.SetColumnName(ToSnakeCase(property.Name));
            }

            foreach (var key in entity.GetKeys())
                key.SetName(ToSnakeCase(key.GetName() ?? ""));

            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? ""));
        }

        // ---------- UTC CONVERTER ----------
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
            v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime()
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(utcConverter);
                }
            }
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var startUnderscores = System.Text.RegularExpressions.Regex.Match(input, @"^_+");
        return startUnderscores + System.Text.RegularExpressions.Regex
            .Replace(input, @"([a-z0-9])([A-Z])", "$1_$2")
            .ToLower();
    }
}
