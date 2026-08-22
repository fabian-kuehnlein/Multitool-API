using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Infrastructure.Repositories;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Infrastructure.Tests;

public class WorkDayRepositoryTests : RepositoryTestBase
{
    private readonly WorkDayRepository _sut;

    public WorkDayRepositoryTests()
    {
        _sut = new WorkDayRepository(Context);
    }

    private static WorkDay CreateWorkDay(DateTime date, DayStatus status = DayStatus.Normal)
    {
        var workDay = WorkTimePlannerTestData.DefaultWorkDay;
        workDay.Id = 0;
        workDay.Date = date;
        workDay.Status = status;
        return workDay;
    }

    // CreateWorkDayAsync

    [Fact]
    public async Task CreateWorkDayAsync_WhenWorkDayIsValid_AddsWorkDayToDatabase()
    {
        // Arrange
        var workDay = WorkTimePlannerTestData.DefaultWorkDay;

        // Act
        await _sut.CreateWorkDayAsync(workDay);

        // Assert
        var dbWorkDay = await Context.WorkDays.AsNoTracking().FirstOrDefaultAsync(w => w.Id == workDay.Id);
        Assert.NotNull(dbWorkDay);
        AssertEx.AreEqual(workDay.Date, dbWorkDay!.Date);
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenWorkDayExists_ReturnsWorkDay()
    {
        // Arrange
        var workDay = WorkTimePlannerTestData.DefaultWorkDay;
        Context.WorkDays.Add(workDay);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(workDay.Id);

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(workDay.Id, result!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenWorkDayDoesNotExist_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    // GetByDateRangeAsync

    [Fact]
    public async Task GetByDateRangeAsync_WhenWorkDaysExistInRange_ReturnsWorkDays()
    {
        // Arrange
        var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        Context.WorkDays.AddRange(
            CreateWorkDay(start),
            CreateWorkDay(start.AddDays(1)),
            CreateWorkDay(start.AddDays(7))
        );
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByDateRangeAsync(start, start.AddDays(3));

        // Assert
        AssertEx.AreEqual(2, result.Count);
    }

    [Fact]
    public async Task GetByDateRangeAsync_WhenWorkDayIsOnEndDate_IsExcluded()
    {
        // Arrange
        var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        Context.WorkDays.Add(CreateWorkDay(start.AddDays(3)));
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByDateRangeAsync(start, start.AddDays(3));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByDateRangeAsync_WhenWorkDaysExist_ReturnsSortedByDate()
    {
        // Arrange
        var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        Context.WorkDays.AddRange(
            CreateWorkDay(start.AddDays(2)),
            CreateWorkDay(start),
            CreateWorkDay(start.AddDays(1))
        );
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByDateRangeAsync(start, start.AddDays(7));

        // Assert
        AssertEx.AreEqual(
            new List<DateTime> { start, start.AddDays(1), start.AddDays(2) },
            result.Select(w => w.Date).ToList());
    }

    // UpdateWorkDayAsync

    [Fact]
    public async Task UpdateWorkDayAsync_WhenWorkDayExists_UpdatesFields()
    {
        // Arrange
        var workDay = WorkTimePlannerTestData.DefaultWorkDay;
        Context.WorkDays.Add(workDay);
        await Context.SaveChangesAsync();

        workDay.Status = DayStatus.Sick;
        workDay.IsHomeOffice = true;

        // Act
        await _sut.UpdateWorkDayAsync(workDay);

        // Assert
        var dbWorkDay = await Context.WorkDays.AsNoTracking().FirstOrDefaultAsync(w => w.Id == workDay.Id);
        Assert.NotNull(dbWorkDay);
        AssertEx.AreEqual(DayStatus.Sick, dbWorkDay!.Status);
        Assert.True(dbWorkDay.IsHomeOffice);
    }

    // DeleteWorkDayAsync

    [Fact]
    public async Task DeleteWorkDayAsync_WhenWorkDayExists_RemovesWorkDay()
    {
        // Arrange
        var workDay = WorkTimePlannerTestData.DefaultWorkDay;
        Context.WorkDays.Add(workDay);
        await Context.SaveChangesAsync();

        // Act
        await _sut.DeleteWorkDayAsync(workDay);

        // Assert
        var dbWorkDay = await Context.WorkDays.AsNoTracking().FirstOrDefaultAsync(w => w.Id == workDay.Id);
        Assert.Null(dbWorkDay);
    }
}
