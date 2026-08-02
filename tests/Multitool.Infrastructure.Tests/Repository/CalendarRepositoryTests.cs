using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Entities.Category;
using Multitool.Infrastructure.Repositories;
using Multitool.Tests.Shared;

namespace Multitool.Infrastructure.Tests;

public class CalendarRepositoryTests : RepositoryTestBase
{
    private readonly CalendarRepository _sut;

    private static readonly DateTime Start = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly Category DefaultCategory = CalendarTestData.DefaultCategory;

    public CalendarRepositoryTests()
    {
        _sut = new CalendarRepository(Context);
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenEventExists_ReturnsEvent()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        var ev = CalendarTestData.DefaultEvent;
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(ev.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(ev.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEventDoesNotExist_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // GetEventsByRangeAsync

    [Fact]
    public async Task GetEventsByRangeAsync_WhenEventFullyWithinRange_ReturnsEvent()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        await Context.SaveChangesAsync();

        var ev = new CalendarEvent
        {
            Title = "Meeting",
            StartDateTime = Start,
            EndDateTime = Start.AddHours(1),
            CategoryId = DefaultCategory.Id,
            IsAllDay = false
        };
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsByRangeAsync(Start.AddDays(-1), Start.AddDays(1), "");

        // Assert
        results.Should().ContainSingle(e => e.Title == "Meeting");
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenEventStartsBeforeRangeAndEndsInside_IsIncluded()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "Overlap",
            StartDateTime = Start.AddHours(-1),
            EndDateTime = Start.AddHours(1),
            CategoryId = DefaultCategory.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsByRangeAsync(Start, Start.AddDays(1), "");

        // Assert
        results.Should().ContainSingle(e => e.Title == "Overlap");
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenEventEndsBeforeRangeStart_IsExcluded()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "PastEvent",
            StartDateTime = Start.AddDays(-5),
            EndDateTime = Start.AddDays(-4),
            CategoryId = DefaultCategory.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsByRangeAsync(Start, Start.AddDays(1), "");

        // Assert
        results.Should().NotContain(e => e.Title == "PastEvent");
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenRecurringEventHasNoRecurrenceEnd_IsIncluded()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "Recurring",
            StartDateTime = Start.AddYears(-1),
            EndDateTime = Start.AddYears(-1).AddHours(1),
            RecurrenceRule = "FREQ=WEEKLY",
            RecurrenceEnd = null,
            CategoryId = DefaultCategory.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsByRangeAsync(Start, Start.AddDays(1), "");

        // Assert
        results.Should().ContainSingle(e => e.Title == "Recurring");
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenRecurringEventEndsBeforeRangeStart_IsExcluded()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "EndedRecurring",
            StartDateTime = Start.AddYears(-1),
            EndDateTime = Start.AddYears(-1).AddHours(1),
            RecurrenceRule = "FREQ=WEEKLY",
            RecurrenceEnd = Start.AddDays(-1),
            CategoryId = DefaultCategory.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsByRangeAsync(Start, Start.AddDays(1), "");

        // Assert
        results.Should().NotContain(e => e.Title == "EndedRecurring");
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenCategoriesProvided_FiltersByCategory()
    {
        // Arrange
        var category2 = new Category { Id = 2, Name = "Familie", Color = "#5d26b6" };
        Context.Categories.AddRange(DefaultCategory, category2);
        Context.CalendarEvents.AddRange(
            new CalendarEvent { Title = "Cat1", StartDateTime = Start, EndDateTime = Start.AddHours(1), CategoryId = DefaultCategory.Id, IsAllDay = false },
            new CalendarEvent { Title = "Cat2", StartDateTime = Start, EndDateTime = Start.AddHours(1), CategoryId = category2.Id, IsAllDay = false }
        );
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsByRangeAsync(Start.AddHours(-1), Start.AddHours(2), DefaultCategory.Id.ToString());

        // Assert
        results.Should().ContainSingle(e => e.Title == "Cat1");
        results.Should().NotContain(e => e.Title == "Cat2");
    }

    // SearchCalendarEventsAsync

    [Fact]
    public async Task SearchCalendarEventsAsync_WhenTitleMatches_ReturnsEvent()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        Context.CalendarEvents.Add(new CalendarEvent { Title = "Team Meeting", StartDateTime = DateTime.UtcNow, CategoryId = DefaultCategory.Id, IsAllDay = false });
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.SearchCalendarEventsAsync("meeting");

        // Assert
        results.Should().ContainSingle(e => e.Title == "Team Meeting");
    }

    [Fact]
    public async Task SearchCalendarEventsAsync_WhenNoteMatches_ReturnsEvent()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        Context.CalendarEvents.Add(new CalendarEvent { Title = "Other", Note = "Important discussion", StartDateTime = DateTime.UtcNow, CategoryId = DefaultCategory.Id, IsAllDay = false });
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.SearchCalendarEventsAsync("discussion");

        // Assert
        results.Should().ContainSingle(e => e.Title == "Other");
    }

    [Fact]
    public async Task SearchCalendarEventsAsync_WhenNoMatch_ReturnsEmptyList()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        Context.CalendarEvents.Add(new CalendarEvent { Title = "Other", StartDateTime = DateTime.UtcNow, CategoryId = DefaultCategory.Id, IsAllDay = false });
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.SearchCalendarEventsAsync("nonexistent");

        // Assert
        results.Should().BeEmpty();
    }

    // InsertEventAsync

    [Fact]
    public async Task InsertEventAsync_WhenEventIsValid_AddsEvent()
    {
        // Arrange
        var ev = CalendarTestData.DefaultEvent;

        Context.Categories.Add(DefaultCategory);

        // Act
        var id = await _sut.InsertEventAsync(ev);

        // Assert
        id.Should().BeGreaterThan(0);
        var dbEv = await Context.CalendarEvents.FindAsync((int)id);
        dbEv.Should().NotBeNull();
        dbEv!.Title.Should().Be(ev.Title);
    }

    // UpdateEventAsync

    [Fact]
    public async Task UpdateEventAsync_WhenEventExists_UpdatesEvent()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        var ev = new CalendarEvent { Title = "Original", StartDateTime = DateTime.UtcNow, CategoryId = DefaultCategory.Id, IsAllDay = false };
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        ev.Title = "Updated";

        // Act
        await _sut.UpdateEventAsync(ev);

        // Assert
        var dbEv = await Context.CalendarEvents.FindAsync(ev.Id);
        dbEv!.Title.Should().Be("Updated");
    }

    // DeleteEventAsync

    [Fact]
    public async Task DeleteEventAsync_WhenEventExists_RemovesEvent()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        var ev = new CalendarEvent { Title = "ToDelete", StartDateTime = DateTime.UtcNow, CategoryId = DefaultCategory.Id, IsAllDay = false };
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        // Act
        await _sut.DeleteEventAsync(ev.Id);

        // Assert
        var dbEv = await Context.CalendarEvents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == ev.Id);
        dbEv.Should().BeNull();
    }

    // GetEventsOlderThanAsync

    [Fact]
    public async Task GetEventsOlderThanAsync_WhenNonRecurringEventEndedBeforeThreshold_ReturnsEvent()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        var threshold = DateTime.UtcNow.AddMonths(-3);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "Old",
            StartDateTime = threshold.AddDays(-10),
            EndDateTime = threshold.AddDays(-5),
            CategoryId = DefaultCategory.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsOlderThanAsync(threshold);

        // Assert
        results.Should().ContainSingle(e => e.Title == "Old");
    }

    [Fact]
    public async Task GetEventsOlderThanAsync_WhenRecurringEventHasRecurrenceEndBeforeThreshold_ReturnsEvent()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        var threshold = DateTime.UtcNow.AddMonths(-3);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "OldRecurring",
            StartDateTime = threshold.AddYears(-1),
            EndDateTime = threshold.AddYears(-1).AddHours(1),
            RecurrenceRule = "FREQ=WEEKLY",
            RecurrenceEnd = threshold.AddDays(-1),
            CategoryId = DefaultCategory.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsOlderThanAsync(threshold);

        // Assert
        results.Should().ContainSingle(e => e.Title == "OldRecurring");
    }

    [Fact]
    public async Task GetEventsOlderThanAsync_WhenRecurringEventHasNoRecurrenceEnd_IsExcluded()
    {
        // Arrange
        Context.Categories.Add(DefaultCategory);
        var threshold = DateTime.UtcNow.AddMonths(-3);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "OngoingRecurring",
            StartDateTime = threshold.AddYears(-1),
            EndDateTime = threshold.AddYears(-1).AddHours(1),
            RecurrenceRule = "FREQ=WEEKLY",
            RecurrenceEnd = null,
            CategoryId = DefaultCategory.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsOlderThanAsync(threshold);

        // Assert
        results.Should().NotContain(e => e.Title == "OngoingRecurring");
    }
}
