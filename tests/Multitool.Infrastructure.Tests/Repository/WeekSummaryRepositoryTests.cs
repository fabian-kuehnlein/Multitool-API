using FluentAssertions;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Infrastructure.Repositories;

namespace Multitool.Infrastructure.Tests;

public class WeekSummaryRepositoryTests : RepositoryTestBase
{
    private readonly WeekSummaryRepository _sut;

    public WeekSummaryRepositoryTests()
    {
        _sut = new WeekSummaryRepository(Context);
    }

    // AddAsync

    [Fact]
    public async Task AddAsync_WhenSummaryIsValid_AddsSummaryToDatabase()
    {
        // Arrange
        var summary = new WeekSummary { Year = 2026, WeekNumber = 23, TotalOvertime = 60 };

        // Act
        await _sut.AddAsync(summary);

        // Assert
        var dbSummary = await Context.WeekSummaries.FindAsync(summary.Id);
        dbSummary.Should().NotBeNull();
        dbSummary!.TotalOvertime.Should().Be(60);
    }

    // GetByYearAndWeekAsync

    [Fact]
    public async Task GetByYearAndWeekAsync_WhenSummaryExists_ReturnsSummary()
    {
        // Arrange
        var summary = new WeekSummary { Year = 2026, WeekNumber = 23, TotalOvertime = 60 };
        Context.WeekSummaries.Add(summary);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByYearAndWeekAsync(2026, 23);

        // Assert
        result.Should().NotBeNull();
        result!.TotalOvertime.Should().Be(60);
    }

    [Fact]
    public async Task GetByYearAndWeekAsync_WhenSummaryDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByYearAndWeekAsync(2026, 23);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByYearAndWeekAsync_WhenOtherYearAndWeekExist_ReturnsNull()
    {
        // Arrange
        Context.WeekSummaries.Add(new WeekSummary { Year = 2025, WeekNumber = 23, TotalOvertime = 60 });
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByYearAndWeekAsync(2026, 23);

        // Assert
        result.Should().BeNull();
    }

    // GetPreviousWeekSummaryAsync

    [Fact]
    public async Task GetPreviousWeekSummaryAsync_WhenPreviousWeekInSameYearExists_ReturnsMostRecentPrevious()
    {
        // Arrange
        Context.WeekSummaries.AddRange(
            new WeekSummary { Year = 2026, WeekNumber = 20, TotalOvertime = 10 },
            new WeekSummary { Year = 2026, WeekNumber = 21, TotalOvertime = 20 },
            new WeekSummary { Year = 2026, WeekNumber = 22, TotalOvertime = 30 }
        );
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetPreviousWeekSummaryAsync(2026, 23);

        // Assert
        result.Should().NotBeNull();
        result!.WeekNumber.Should().Be(22);
        result.TotalOvertime.Should().Be(30);
    }

    [Fact]
    public async Task GetPreviousWeekSummaryAsync_WhenPreviousWeekIsInPreviousYear_ReturnsPreviousYearSummary()
    {
        // Arrange
        Context.WeekSummaries.Add(new WeekSummary { Year = 2025, WeekNumber = 52, TotalOvertime = 45 });
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetPreviousWeekSummaryAsync(2026, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Year.Should().Be(2025);
        result.WeekNumber.Should().Be(52);
    }

    [Fact]
    public async Task GetPreviousWeekSummaryAsync_WhenNoSummaryExistsBefore_ReturnsNull()
    {
        // Arrange
        Context.WeekSummaries.Add(new WeekSummary { Year = 2026, WeekNumber = 24, TotalOvertime = 60 });
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetPreviousWeekSummaryAsync(2026, 23);

        // Assert
        result.Should().BeNull();
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenSummaryExists_UpdatesTotalOvertime()
    {
        // Arrange
        var summary = new WeekSummary { Year = 2026, WeekNumber = 23, TotalOvertime = 0 };
        Context.WeekSummaries.Add(summary);
        await Context.SaveChangesAsync();

        // Act
        summary.TotalOvertime = 120;
        await _sut.UpdateAsync(summary);

        // Assert
        var dbSummary = await Context.WeekSummaries.FindAsync(summary.Id);
        dbSummary!.TotalOvertime.Should().Be(120);
    }
}
