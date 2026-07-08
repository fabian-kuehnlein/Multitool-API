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

    public CalendarRepositoryTests()
    {
        _sut = new CalendarRepository(Context);
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenEventExists_ReturnsEvent()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        var ev = CalendarTestData.DefaultEvent;
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(ev.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be(ev.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEventDoesNotExist_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    // GetEventsByRangeAsync

    [Fact]
    public async Task GetEventsByRangeAsync_WhenEventFullyWithinRange_ReturnsEvent()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        var start = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var ev = new CalendarEvent
        {
            Title = "Meeting",
            StartDateTime = start,
            EndDateTime = start.AddHours(1),
            CategoryId = category.Id,
            IsAllDay = false
        };
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        var results = await _sut.GetEventsByRangeAsync(start.AddDays(-1), start.AddDays(1), "");

        results.Should().ContainSingle(e => e.Title == "Meeting");
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenEventStartsBeforeRangeAndEndsInside_IsIncluded()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        var start = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "Overlap",
            StartDateTime = start.AddHours(-1),
            EndDateTime = start.AddHours(1),
            CategoryId = category.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        var results = await _sut.GetEventsByRangeAsync(start, start.AddDays(1), "");

        results.Should().ContainSingle(e => e.Title == "Overlap");
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenEventEndsBeforeRangeStart_IsExcluded()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        var start = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "PastEvent",
            StartDateTime = start.AddDays(-5),
            EndDateTime = start.AddDays(-4),
            CategoryId = category.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        var results = await _sut.GetEventsByRangeAsync(start, start.AddDays(1), "");

        results.Should().NotContain(e => e.Title == "PastEvent");
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenRecurringEventHasNoRecurrenceEnd_IsIncluded()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        var start = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "Recurring",
            StartDateTime = start.AddYears(-1),
            EndDateTime = start.AddYears(-1).AddHours(1),
            RecurrenceRule = "FREQ=WEEKLY",
            RecurrenceEnd = null,
            CategoryId = category.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        var results = await _sut.GetEventsByRangeAsync(start, start.AddDays(1), "");

        results.Should().ContainSingle(e => e.Title == "Recurring");
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenRecurringEventEndsBeforeRangeStart_IsExcluded()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        var start = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "EndedRecurring",
            StartDateTime = start.AddYears(-1),
            EndDateTime = start.AddYears(-1).AddHours(1),
            RecurrenceRule = "FREQ=WEEKLY",
            RecurrenceEnd = start.AddDays(-1),
            CategoryId = category.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        var results = await _sut.GetEventsByRangeAsync(start, start.AddDays(1), "");

        results.Should().NotContain(e => e.Title == "EndedRecurring");
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenCategoriesProvided_FiltersByCategory()
    {
        var category1 = CalendarTestData.DefaultCategory;
        var category2 = new Category{ Id = 2, Name = "Familie", Color = "#5d26b6" };
        Context.Categories.AddRange(category1, category2);
        var start = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        Context.CalendarEvents.AddRange(
            new CalendarEvent { Title = "Cat1", StartDateTime = start, EndDateTime = start.AddHours(1), CategoryId = category1.Id, IsAllDay = false },
            new CalendarEvent { Title = "Cat2", StartDateTime = start, EndDateTime = start.AddHours(1), CategoryId = category2.Id, IsAllDay = false }
        );
        await Context.SaveChangesAsync();

        var results = await _sut.GetEventsByRangeAsync(start.AddHours(-1), start.AddHours(2), category1.Id.ToString());

        results.Should().ContainSingle(e => e.Title == "Cat1");
        results.Should().NotContain(e => e.Title == "Cat2");
    }

    // SearchCalendarEventsAsync

    [Fact]
    public async Task SearchCalendarEventsAsync_WhenTitleMatches_ReturnsEvent()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        Context.CalendarEvents.Add(new CalendarEvent { Title = "Team Meeting", StartDateTime = DateTime.UtcNow, CategoryId = category.Id, IsAllDay = false });
        await Context.SaveChangesAsync();

        var results = await _sut.SearchCalendarEventsAsync("meeting");

        results.Should().ContainSingle(e => e.Title == "Team Meeting");
    }

    [Fact]
    public async Task SearchCalendarEventsAsync_WhenNoteMatches_ReturnsEvent()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        Context.CalendarEvents.Add(new CalendarEvent { Title = "Other", Note = "Important discussion", StartDateTime = DateTime.UtcNow, CategoryId = category.Id, IsAllDay = false });
        await Context.SaveChangesAsync();

        var results = await _sut.SearchCalendarEventsAsync("discussion");

        results.Should().ContainSingle(e => e.Title == "Other");
    }

    [Fact]
    public async Task SearchCalendarEventsAsync_WhenNoMatch_ReturnsEmptyList()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        Context.CalendarEvents.Add(new CalendarEvent { Title = "Other", StartDateTime = DateTime.UtcNow, CategoryId = category.Id, IsAllDay = false });
        await Context.SaveChangesAsync();

        var results = await _sut.SearchCalendarEventsAsync("nonexistent");

        results.Should().BeEmpty();
    }

    // InsertEventAsync

    [Fact]
    public async Task InsertEventAsync_AddsEvent()
    {
        var ev = CalendarTestData.DefaultEvent;
        var category = CalendarTestData.DefaultCategory;

        Context.Categories.Add(category);

        var id = await _sut.InsertEventAsync(ev);

        id.Should().BeGreaterThan(0);
        var dbEv = await Context.CalendarEvents.FindAsync((int)id);
        dbEv.Should().NotBeNull();
        dbEv!.Title.Should().Be(ev.Title);
    }

    // UpdateEventAsync

    [Fact]
    public async Task UpdateEventAsync_UpdatesExistingEvent()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        var ev = new CalendarEvent { Title = "Original", StartDateTime = DateTime.UtcNow, CategoryId = category.Id, IsAllDay = false };
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        ev.Title = "Updated";
        await _sut.UpdateEventAsync(ev);

        var dbEv = await Context.CalendarEvents.FindAsync(ev.Id);
        dbEv!.Title.Should().Be("Updated");
    }

    // DeleteEventAsync

    [Fact]
    public async Task DeleteEventAsync_RemovesEvent()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        var ev = new CalendarEvent { Title = "ToDelete", StartDateTime = DateTime.UtcNow, CategoryId = category.Id, IsAllDay = false };
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        await _sut.DeleteEventAsync(ev.Id);

        var dbEv = await Context.CalendarEvents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == ev.Id);
        dbEv.Should().BeNull();
    }

    // GetEventsOlderThanAsync

    [Fact]
    public async Task GetEventsOlderThanAsync_WhenNonRecurringEventEndedBeforeThreshold_ReturnsEvent()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        var threshold = DateTime.UtcNow.AddMonths(-3);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "Old",
            StartDateTime = threshold.AddDays(-10),
            EndDateTime = threshold.AddDays(-5),
            CategoryId = category.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        var results = await _sut.GetEventsOlderThanAsync(threshold);

        results.Should().ContainSingle(e => e.Title == "Old");
    }

    [Fact]
    public async Task GetEventsOlderThanAsync_WhenRecurringEventHasRecurrenceEndBeforeThreshold_ReturnsEvent()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        var threshold = DateTime.UtcNow.AddMonths(-3);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "OldRecurring",
            StartDateTime = threshold.AddYears(-1),
            EndDateTime = threshold.AddYears(-1).AddHours(1),
            RecurrenceRule = "FREQ=WEEKLY",
            RecurrenceEnd = threshold.AddDays(-1),
            CategoryId = category.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        var results = await _sut.GetEventsOlderThanAsync(threshold);

        results.Should().ContainSingle(e => e.Title == "OldRecurring");
    }

    [Fact]
    public async Task GetEventsOlderThanAsync_WhenRecurringEventHasNoRecurrenceEnd_IsExcluded()
    {
        var category = CalendarTestData.DefaultCategory;
        Context.Categories.Add(category);
        var threshold = DateTime.UtcNow.AddMonths(-3);
        Context.CalendarEvents.Add(new CalendarEvent
        {
            Title = "OngoingRecurring",
            StartDateTime = threshold.AddYears(-1),
            EndDateTime = threshold.AddYears(-1).AddHours(1),
            RecurrenceRule = "FREQ=WEEKLY",
            RecurrenceEnd = null,
            CategoryId = category.Id,
            IsAllDay = false
        });
        await Context.SaveChangesAsync();

        var results = await _sut.GetEventsOlderThanAsync(threshold);

        results.Should().NotContain(e => e.Title == "OngoingRecurring");
    }
}
