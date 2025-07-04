using Microsoft.EntityFrameworkCore;
using MultitoolApi.Businesslogic.Models;
using MultitoolApi.Infrastructure.DataAccessLayer.Models.CustomTable;

namespace MultitoolApi.ConfigModels;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<CalendarEvent> CalendarEvents { get; set; }
    public DbSet<Category> Categories { get;  set; }
    public DbSet<CustomTable> CustomTables { get; set; }
    public DbSet<CustomColumn> CustomColumns { get; set; }
    public DbSet<CustomRow> CustomRows { get; set; }
    public DbSet<CustomCell> CustomCells { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CalendarEvent>(e =>
        {
            e.ToTable("calendar_events");

            e.HasKey(e => e.EventId);

            e.Property(e => e.EventId)
                .HasColumnName("eventId")
                .ValueGeneratedOnAdd(); // AUTO_INCREMENT

            e.Property(e => e.EventTitle)
                .HasColumnName("eventTitle")
                .IsRequired(); // NOT NULL

            e.Property(e => e.EventNote)
                .HasColumnName("eventNote");

            e.Property(e => e.StartDateTime)
                .HasColumnName("startDateTime")
                .IsRequired();

            e.Property(e => e.EndDateTime)
                .HasColumnName("endDateTime");

            e.Property(e => e.IsAllDay)
                .HasColumnName("isAllDay")
                .IsRequired();

            e.Property(e => e.CategoryId)
                .HasColumnName("categoryId")
                .IsRequired();

            e.Property(e => e.RecurrenceRule)
                .HasColumnName("recurrenceRule");

            e.Property(e => e.RecurrenceEnd)
                .HasColumnName("recurrenceEnd");

            e.HasOne<Category>()
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .HasConstraintName("FK_calendar_events_categoryId");
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories");

            e.HasKey(c => c.CategoryId);

            e.Property(c => c.CategoryId)
                .HasColumnName("categoryId")
                .ValueGeneratedOnAdd();

            e.Property(c => c.CategoryName)
                .HasColumnName("categoryName")
                .IsRequired();
        });

        modelBuilder.Entity<CustomTable>(e =>
        {
            e.ToTable("custom_table");
            e.HasKey(t => t.TableId);

            e.Property(t => t.TableId)
              .HasColumnName("tableId")
              .ValueGeneratedOnAdd(); // AUTO_INCREMENT

            e.Property(t => t.Name)
              .HasColumnName("tableName")
              .HasMaxLength(120)
              .IsRequired();

            e.Property(t => t.CreatedAt)
              .HasColumnName("created_at")
              .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ---------- COLUMN ----------
        modelBuilder.Entity<CustomColumn>(e =>
        {
            e.ToTable("custom_column");
            e.HasKey(c => c.ColumnId);

            e.HasOne(c => c.Table)
             .WithMany(t => t.Columns)
             .HasForeignKey(c => c.TableId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(c => c.Name)
             .HasColumnName("name")
             .HasMaxLength(120)
             .IsRequired();

            e.Property(c => c.ColOrder)
             .HasColumnName("col_order");

            // CLR Enum  ⇄  MySQL ENUM
            e.Property(c => c.DataType)
             .HasColumnName("data_type")
             .HasColumnType("enum('string','int','decimal','date','bool')")
             .HasConversion(
                 v => v.ToString().ToLower(),                    // CLR → DB
                 v => Enum.Parse<CustomDataType>(v, true))       // DB  → CLR
             .IsRequired();
        });

        // ---------- ROW ----------
        modelBuilder.Entity<CustomRow>(e =>
        {
            e.ToTable("custom_row");
            e.HasKey(r => r.RowId);

            e.HasOne(r => r.Table)
             .WithMany(t => t.Rows)
             .HasForeignKey(r => r.TableId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(r => r.CreatedAt)
             .HasColumnName("created_at")
             .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ---------- CELL ----------
        modelBuilder.Entity<CustomCell>(e =>
        {
            e.ToTable("custom_cell");

            // Composite PK  (RowId + ColumnId)
            e.HasKey(c => new { c.RowId, c.ColumnId });

            e.HasOne(c => c.Row)
             .WithMany(r => r.Cells)
             .HasForeignKey(c => c.RowId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.Column)
             .WithMany()
             .HasForeignKey(c => c.ColumnId)
             .OnDelete(DeleteBehavior.Cascade);

            // Value‑Spalten → snake_case
            e.Property(c => c.ValString).HasColumnName("val_string");
            e.Property(c => c.ValInt   ).HasColumnName("val_int");
            e.Property(c => c.ValDec   ).HasColumnName("val_dec");
            e.Property(c => c.ValDate  ).HasColumnName("val_date");
            e.Property(c => c.ValBool  ).HasColumnName("val_bool");

            // Typ‑spezifische Indizes
            e.HasIndex(c => new { c.ColumnId, c.ValInt  }).HasDatabaseName("idx_cell_int");
            e.HasIndex(c => new { c.ColumnId, c.ValDec  }).HasDatabaseName("idx_cell_dec");
            e.HasIndex(c => new { c.ColumnId, c.ValDate }).HasDatabaseName("idx_cell_date");
        });
    }
}