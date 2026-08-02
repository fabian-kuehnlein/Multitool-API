using FluentAssertions;
using Multitool.Domain.Entities.WorkTimePlanner;
using Multitool.Infrastructure.Repositories;

namespace Multitool.Infrastructure.Tests;

public class WorkTimeSettingsRepositoryTests : RepositoryTestBase
{
    private readonly WorkTimeSettingsRepository _sut;

    public WorkTimeSettingsRepositoryTests()
    {
        _sut = new WorkTimeSettingsRepository(Context);
    }

    // GetAsync

    [Fact]
    public async Task GetAsync_WhenSettingsExist_ReturnsSettings()
    {
        // Arrange
        var settings = new WorkTimeSettings { DailyTargetMinutes = 480 };
        Context.WorkTimeSettings.Add(settings);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAsync();

        // Assert
        result.Should().NotBeNull();
        result!.DailyTargetMinutes.Should().Be(480);
    }

    [Fact]
    public async Task GetAsync_WhenNoSettingsExist_ReturnsNull()
    {
        // Act
        var result = await _sut.GetAsync();

        // Assert
        result.Should().BeNull();
    }

    // AddAsync

    [Fact]
    public async Task AddAsync_WhenSettingsAreValid_AddsSettingsToDatabase()
    {
        // Arrange
        var settings = new WorkTimeSettings
        {
            DailyTargetMinutes = 450,
            BreakRule6h = 20,
            BreakRule9h = 40,
            HomeOfficeLimit = 15
        };

        // Act
        await _sut.AddAsync(settings);

        // Assert
        var dbSettings = await Context.WorkTimeSettings.FindAsync(settings.Id);
        dbSettings.Should().NotBeNull();
        dbSettings!.DailyTargetMinutes.Should().Be(450);
        dbSettings.BreakRule6h.Should().Be(20);
        dbSettings.BreakRule9h.Should().Be(40);
        dbSettings.HomeOfficeLimit.Should().Be(15);
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenSettingsExist_UpdatesAllFields()
    {
        // Arrange
        var settings = new WorkTimeSettings { DailyTargetMinutes = 480, BreakRule6h = 30, BreakRule9h = 45, HomeOfficeLimit = 20 };
        Context.WorkTimeSettings.Add(settings);
        await Context.SaveChangesAsync();

        // Act
        settings.DailyTargetMinutes = 450;
        settings.BreakRule6h = 20;
        settings.BreakRule9h = 40;
        settings.HomeOfficeLimit = 15;
        await _sut.UpdateAsync(settings);

        // Assert
        var dbSettings = await Context.WorkTimeSettings.FindAsync(settings.Id);
        dbSettings!.DailyTargetMinutes.Should().Be(450);
        dbSettings.BreakRule6h.Should().Be(20);
        dbSettings.BreakRule9h.Should().Be(40);
        dbSettings.HomeOfficeLimit.Should().Be(15);
    }
}
