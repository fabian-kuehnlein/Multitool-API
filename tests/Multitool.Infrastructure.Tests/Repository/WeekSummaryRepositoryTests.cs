using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Infrastructure.Repositories;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Infrastructure.Tests;

public class WeekSummaryRepositoryTests : RepositoryTestBase
{
    private readonly WeekSummaryRepository _sut;

    public WeekSummaryRepositoryTests()
    {
        _sut = new WeekSummaryRepository(Context);
    }

    private static WeekSummary CreateSummary(int year, int weekNumber, int totalOvertime)
    {
        var summary = WorkTimePlannerTestData.DefaultWeekSummary;
        summary.Id = 0;
        summary.Year = year;
        summary.WeekNumber = weekNumber;
        summary.TotalOvertime = totalOvertime;
        return summary;
    }

    // AddAsync

    [Fact]
    public async Task AddAsync_WhenSummaryIsValid_AddsSummaryToDatabase()
    {
        // Arrange
        var summary = CreateSummary(2026, 23, 60);

        // Act
        await _sut.AddAsync(summary);

        // Assert
        var dbSummary = await Context.WeekSummaries.AsNoTracking().FirstOrDefaultAsync(s => s.Id == summary.Id);
        Assert.NotNull(dbSummary);
        AssertEx.AreEqual(60, dbSummary!.TotalOvertime);
    }

    // GetByYearAndWeekAsync

    [Fact]
    public async Task GetByYearAndWeekAsync_WhenSummaryExists_ReturnsSummary()
    {
        // Arrange
        Context.WeekSummaries.Add(CreateSummary(2026, 23, 60));
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByYearAndWeekAsync(2026, 23);

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(60, result!.TotalOvertime);
    }

    [Fact]
    public async Task GetByYearAndWeekAsync_WhenSummaryDoesNotExist_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _sut.GetByYearAndWeekAsync(2026, 23);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByYearAndWeekAsync_WhenOtherYearAndWeekExist_ReturnsNull()
    {
        // Arrange
        Context.WeekSummaries.Add(CreateSummary(2025, 23, 60));
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByYearAndWeekAsync(2026, 23);

        // Assert
        Assert.Null(result);
    }

    // GetPreviousWeekSummaryAsync

    [Fact]
    public async Task GetPreviousWeekSummaryAsync_WhenPreviousWeekInSameYearExists_ReturnsMostRecentPrevious()
    {
        // Arrange
        Context.WeekSummaries.AddRange(
            CreateSummary(2026, 20, 10),
            CreateSummary(2026, 21, 20),
            CreateSummary(2026, 22, 30)
        );
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetPreviousWeekSummaryAsync(2026, 23);

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(22, result!.WeekNumber);
        AssertEx.AreEqual(30, result.TotalOvertime);
    }

    [Fact]
    public async Task GetPreviousWeekSummaryAsync_WhenPreviousWeekIsInPreviousYear_ReturnsPreviousYearSummary()
    {
        // Arrange
        Context.WeekSummaries.Add(CreateSummary(2025, 52, 45));
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetPreviousWeekSummaryAsync(2026, 1);

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(2025, result!.Year);
        AssertEx.AreEqual(52, result.WeekNumber);
    }

    [Fact]
    public async Task GetPreviousWeekSummaryAsync_WhenNoSummaryExistsBefore_ReturnsNull()
    {
        // Arrange
        Context.WeekSummaries.Add(CreateSummary(2026, 24, 60));
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetPreviousWeekSummaryAsync(2026, 23);

        // Assert
        Assert.Null(result);
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenSummaryExists_UpdatesTotalOvertime()
    {
        // Arrange
        var summary = CreateSummary(2026, 23, 0);
        Context.WeekSummaries.Add(summary);
        await Context.SaveChangesAsync();

        summary.TotalOvertime = 120;

        // Act
        await _sut.UpdateAsync(summary);

        // Assert
        var dbSummary = await Context.WeekSummaries.AsNoTracking().FirstOrDefaultAsync(s => s.Id == summary.Id);
        Assert.NotNull(dbSummary);
        AssertEx.AreEqual(120, dbSummary!.TotalOvertime);
    }
}
