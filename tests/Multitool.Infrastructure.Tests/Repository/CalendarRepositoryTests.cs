using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Calendar;
using Multitool.Infrastructure.Repositories;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Infrastructure.Tests;

public class CalendarRepositoryTests : RepositoryTestBase
{
    private readonly CalendarRepository _sut;

    private static readonly DateTime Start = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);

    public CalendarRepositoryTests()
    {
        _sut = new CalendarRepository(Context);
    }

    private static CalendarEvent CreateEvent(string title, DateTime start, DateTime end)
    {
        var ev = CalendarTestData.DefaultEvent;
        ev.Id = 0;
        ev.Title = title;
        ev.StartDateTime = start;
        ev.EndDateTime = end;
        return ev;
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenEventExists_ReturnsEvent()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        var ev = CalendarTestData.DefaultEvent;
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(ev.Id);

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(ev.Title, result!.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEventDoesNotExist_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    // GetEventsByRangeAsync

    [Fact]
    public async Task GetEventsByRangeAsync_WhenEventFullyWithinRange_ReturnsEvent()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        Context.CalendarEvents.Add(CreateEvent("Meeting", Start, Start.AddHours(1)));
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsByRangeAsync(Start.AddDays(-1), Start.AddDays(1), "");

        // Assert
        AssertEx.AreEqual(1, results.Count);
        AssertEx.AreEqual("Meeting", results[0].Title);
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenEventStartsBeforeRangeAndEndsInside_IsIncluded()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        Context.CalendarEvents.Add(CreateEvent("Overlap", Start.AddHours(-1), Start.AddHours(1)));
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsByRangeAsync(Start, Start.AddDays(1), "");

        // Assert
        AssertEx.AreEqual(1, results.Count);
        AssertEx.AreEqual("Overlap", results[0].Title);
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenEventEndsBeforeRangeStart_IsExcluded()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        Context.CalendarEvents.Add(CreateEvent("PastEvent", Start.AddDays(-5), Start.AddDays(-4)));
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsByRangeAsync(Start, Start.AddDays(1), "");

        // Assert
        Assert.DoesNotContain(results, e => e.Title == "PastEvent");
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenRecurringEventHasNoRecurrenceEnd_IsIncluded()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        var ev = CreateEvent("Recurring", Start.AddYears(-1), Start.AddYears(-1).AddHours(1));
        ev.RecurrenceRule = "FREQ=WEEKLY";
        ev.RecurrenceEnd = null;
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsByRangeAsync(Start, Start.AddDays(1), "");

        // Assert
        AssertEx.AreEqual(1, results.Count);
        AssertEx.AreEqual("Recurring", results[0].Title);
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenRecurringEventEndsBeforeRangeStart_IsExcluded()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        var ev = CreateEvent("EndedRecurring", Start.AddYears(-1), Start.AddYears(-1).AddHours(1));
        ev.RecurrenceRule = "FREQ=WEEKLY";
        ev.RecurrenceEnd = Start.AddDays(-1);
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsByRangeAsync(Start, Start.AddDays(1), "");

        // Assert
        Assert.DoesNotContain(results, e => e.Title == "EndedRecurring");
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenCategoriesProvided_FiltersByCategory()
    {
        // Arrange
        var category2 = CategoryTestData.DefaultCategory;
        category2.Id = 2;
        category2.Name = "Familie";
        Context.Categories.AddRange(CategoryTestData.DefaultCategory, category2);

        var cat1Event = CreateEvent("Cat1", Start, Start.AddHours(1));
        var cat2Event = CreateEvent("Cat2", Start, Start.AddHours(1));
        cat2Event.CategoryId = category2.Id;
        Context.CalendarEvents.AddRange(cat1Event, cat2Event);
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsByRangeAsync(Start.AddHours(-1), Start.AddHours(2), CategoryTestData.DefaultCategory.Id.ToString());

        // Assert
        AssertEx.AreEqual(1, results.Count);
        AssertEx.AreEqual("Cat1", results[0].Title);
        Assert.DoesNotContain(results, e => e.Title == "Cat2");
    }

    // SearchCalendarEventsAsync

    [Fact]
    public async Task SearchCalendarEventsAsync_WhenTitleMatches_ReturnsEvent()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        Context.CalendarEvents.Add(CreateEvent("Team Meeting", DateTime.UtcNow, DateTime.UtcNow.AddHours(1)));
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.SearchCalendarEventsAsync("meeting");

        // Assert
        AssertEx.AreEqual(1, results.Count);
        AssertEx.AreEqual("Team Meeting", results[0].Title);
    }

    [Fact]
    public async Task SearchCalendarEventsAsync_WhenNoteMatches_ReturnsEvent()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        var ev = CreateEvent("Other", DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        ev.Note = "Important discussion";
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.SearchCalendarEventsAsync("discussion");

        // Assert
        AssertEx.AreEqual(1, results.Count);
        AssertEx.AreEqual("Other", results[0].Title);
    }

    [Fact]
    public async Task SearchCalendarEventsAsync_WhenNoMatch_ReturnsEmptyList()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        Context.CalendarEvents.Add(CreateEvent("Other", DateTime.UtcNow, DateTime.UtcNow.AddHours(1)));
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.SearchCalendarEventsAsync("nonexistent");

        // Assert
        Assert.Empty(results);
    }

    // InsertEventAsync

    [Fact]
    public async Task InsertEventAsync_WhenEventIsValid_AddsEvent()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        var ev = CalendarTestData.DefaultEvent;

        // Act
        var id = await _sut.CreateEventAsync(ev);

        // Assert
        Assert.True(id > 0);
        var dbEv = await Context.CalendarEvents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        Assert.NotNull(dbEv);
        AssertEx.AreEqual(ev.Title, dbEv!.Title);
    }

    // UpdateEventAsync

    [Fact]
    public async Task UpdateEventAsync_WhenEventExists_UpdatesEvent()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        var ev = CalendarTestData.DefaultEvent;
        ev.Id = 0;
        ev.Title = "Original";
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        ev.Title = "Updated";

        // Act
        await _sut.UpdateEventAsync(ev);

        // Assert
        var dbEv = await Context.CalendarEvents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == ev.Id);
        Assert.NotNull(dbEv);
        AssertEx.AreEqual("Updated", dbEv!.Title);
    }

    // DeleteEventAsync

    [Fact]
    public async Task DeleteEventAsync_WhenEventExists_RemovesEvent()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        var ev = CalendarTestData.DefaultEvent;
        ev.Id = 0;
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        // Act
        await _sut.DeleteEventAsync(ev.Id);

        // Assert
        var dbEv = await Context.CalendarEvents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == ev.Id);
        Assert.Null(dbEv);
    }

    // GetEventsOlderThanAsync

    [Fact]
    public async Task GetEventsOlderThanAsync_WhenNonRecurringEventEndedBeforeThreshold_ReturnsEvent()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        var threshold = DateTime.UtcNow.AddMonths(-3);
        Context.CalendarEvents.Add(CreateEvent("Old", threshold.AddDays(-10), threshold.AddDays(-5)));
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsOlderThanAsync(threshold);

        // Assert
        AssertEx.AreEqual(1, results.Count);
        AssertEx.AreEqual("Old", results[0].Title);
    }

    [Fact]
    public async Task GetEventsOlderThanAsync_WhenRecurringEventHasRecurrenceEndBeforeThreshold_ReturnsEvent()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        var threshold = DateTime.UtcNow.AddMonths(-3);
        var ev = CreateEvent("OldRecurring", threshold.AddYears(-1), threshold.AddYears(-1).AddHours(1));
        ev.RecurrenceRule = "FREQ=WEEKLY";
        ev.RecurrenceEnd = threshold.AddDays(-1);
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsOlderThanAsync(threshold);

        // Assert
        AssertEx.AreEqual(1, results.Count);
        AssertEx.AreEqual("OldRecurring", results[0].Title);
    }

    [Fact]
    public async Task GetEventsOlderThanAsync_WhenRecurringEventHasNoRecurrenceEnd_IsExcluded()
    {
        // Arrange
        Context.Categories.Add(CategoryTestData.DefaultCategory);
        var threshold = DateTime.UtcNow.AddMonths(-3);
        var ev = CreateEvent("OngoingRecurring", threshold.AddYears(-1), threshold.AddYears(-1).AddHours(1));
        ev.RecurrenceRule = "FREQ=WEEKLY";
        ev.RecurrenceEnd = null;
        Context.CalendarEvents.Add(ev);
        await Context.SaveChangesAsync();

        // Act
        var results = await _sut.GetEventsOlderThanAsync(threshold);

        // Assert
        Assert.DoesNotContain(results, e => e.Title == "OngoingRecurring");
    }
}
