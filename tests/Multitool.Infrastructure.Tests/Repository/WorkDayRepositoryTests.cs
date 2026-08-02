using FluentAssertions;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Infrastructure.Repositories;

namespace Multitool.Infrastructure.Tests;

public class WorkDayRepositoryTests : RepositoryTestBase
{
    private readonly WorkDayRepository _sut;

    public WorkDayRepositoryTests()
    {
        _sut = new WorkDayRepository(Context);
    }

    // AddAsync

    [Fact]
    public async Task AddAsync_WhenWorkDayIsValid_AddsWorkDayToDatabase()
    {
        // Arrange
        var workDay = new WorkDay
        {
            Date = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            Status = DayStatus.Normal
        };

        // Act
        await _sut.AddAsync(workDay);

        // Assert
        var dbWorkDay = await Context.WorkDays.FindAsync(workDay.Id);
        dbWorkDay.Should().NotBeNull();
        dbWorkDay!.Date.Should().Be(workDay.Date);
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenWorkDayExists_ReturnsWorkDay()
    {
        // Arrange
        var workDay = new WorkDay { Date = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), Status = DayStatus.Normal };
        Context.WorkDays.Add(workDay);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(workDay.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(workDay.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenWorkDayDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // GetByDateRangeAsync

    [Fact]
    public async Task GetByDateRangeAsync_WhenWorkDaysExistInRange_ReturnsWorkDays()
    {
        // Arrange
        var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        Context.WorkDays.AddRange(
            new WorkDay { Date = start, Status = DayStatus.Normal },
            new WorkDay { Date = start.AddDays(1), Status = DayStatus.Normal },
            new WorkDay { Date = start.AddDays(7), Status = DayStatus.Normal }
        );
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByDateRangeAsync(start, start.AddDays(3));

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByDateRangeAsync_WhenWorkDayIsOnEndDate_IsExcluded()
    {
        // Arrange
        var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        Context.WorkDays.Add(new WorkDay { Date = start.AddDays(3), Status = DayStatus.Normal });
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByDateRangeAsync(start, start.AddDays(3));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByDateRangeAsync_WhenWorkDaysExist_ReturnsSortedByDate()
    {
        // Arrange
        var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        Context.WorkDays.AddRange(
            new WorkDay { Date = start.AddDays(2), Status = DayStatus.Normal },
            new WorkDay { Date = start, Status = DayStatus.Normal },
            new WorkDay { Date = start.AddDays(1), Status = DayStatus.Normal }
        );
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByDateRangeAsync(start, start.AddDays(7));

        // Assert
        result.Should().BeInAscendingOrder(w => w.Date);
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenWorkDayExists_UpdatesFields()
    {
        // Arrange
        var workDay = new WorkDay { Date = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), Status = DayStatus.Normal };
        Context.WorkDays.Add(workDay);
        await Context.SaveChangesAsync();

        // Act
        workDay.Status = DayStatus.Sick;
        workDay.IsHomeOffice = true;
        await _sut.UpdateAsync(workDay);

        // Assert
        var dbWorkDay = await Context.WorkDays.FindAsync(workDay.Id);
        dbWorkDay!.Status.Should().Be(DayStatus.Sick);
        dbWorkDay.IsHomeOffice.Should().BeTrue();
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_WhenWorkDayExists_RemovesWorkDay()
    {
        // Arrange
        var workDay = new WorkDay { Date = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), Status = DayStatus.Normal };
        Context.WorkDays.Add(workDay);
        await Context.SaveChangesAsync();

        // Act
        await _sut.DeleteAsync(workDay.Id);

        // Assert
        var dbWorkDay = await Context.WorkDays.FindAsync(workDay.Id);
        dbWorkDay.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenWorkDayDoesNotExist_DoesNotThrow()
    {
        // Act
        var act = () => _sut.DeleteAsync(999);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
