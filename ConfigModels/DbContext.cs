using Microsoft.EntityFrameworkCore;
using MultitoolApi.Businesslogic.Models;

namespace MultitoolApi.ConfigModels;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<CalendarEvent> CalendarEvents { get; set; }
    public DbSet<Category> Categories { get;  set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CalendarEvent>(entity =>
        {
            entity.ToTable("calendar_events");

            entity.HasKey(e => e.EventId);

            entity.Property(e => e.EventId)
                .HasColumnName("eventId")
                .ValueGeneratedOnAdd(); // AUTO_INCREMENT

            entity.Property(e => e.EventTitle)
                .HasColumnName("eventTitle")
                .IsRequired(); // NOT NULL

            entity.Property(e => e.EventNote)
                .HasColumnName("eventNote");

            entity.Property(e => e.StartDateTime)
                .HasColumnName("startDateTime")
                .IsRequired();

            entity.Property(e => e.EndDateTime)
                .HasColumnName("endDateTime");

            entity.Property(e => e.IsAllDay)
                .HasColumnName("isAllDay")
                .IsRequired();

            entity.Property(e => e.CategoryId)
                .HasColumnName("categoryId")
                .IsRequired();

            entity.Property(e => e.RecurrenceRule)
                .HasColumnName("recurrenceRule");

            entity.Property(e => e.RecurrenceEnd)
                .HasColumnName("recurrenceEnd");

            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .HasConstraintName("FK_calendar_events_categoryId");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");

            entity.HasKey(c => c.CategoryId);

            entity.Property(c => c.CategoryId)
                .HasColumnName("categoryId")
                .ValueGeneratedOnAdd();

            entity.Property(c => c.CategoryName)
                .HasColumnName("categoryName")
                .IsRequired();
        });
    }
}