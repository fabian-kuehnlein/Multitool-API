using FluentAssertions;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Entities.Category;
using Multitool.Infrastructure.Repositories;

namespace Multitool.Infrastructure.Tests;

public class CalendarRepositoryTests : RepositoryTestBase
{
    private readonly CalendarRepository _sut;

    public CalendarRepositoryTests()
    {
        _sut = new CalendarRepository(Context);
    }

    [Fact]
    public async Task InsertEventAsync_AddsEvent()
    {
        var category = new Category { Id = 1, Name = "Work", Color = "#FF0000" };
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        var ev = new CalendarEvent
        {
            Title = "Meeting",
            StartDateTime = DateTime.UtcNow,
            CategoryId = category.Id,
            IsAllDay = false
        };

        var id = await _sut.InsertEventAsync(ev);

        id.Should().BeGreaterThan(0);
        var dbEv = await Context.CalendarEvents.FindAsync((int)id);
        dbEv.Should().NotBeNull();
        dbEv!.Title.Should().Be("Meeting");
    }

    [Fact]
    public async Task GetEventsByRangeAsync_ReturnsEventsInRange()
    {
        var category = new Category { Id = 2, Name = "Work", Color = "#FF0000" };
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
}
