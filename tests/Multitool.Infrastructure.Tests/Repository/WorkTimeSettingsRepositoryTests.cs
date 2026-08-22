using Microsoft.EntityFrameworkCore;
using Multitool.Infrastructure.Repositories;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

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
        var settings = WorkTimePlannerTestData.DefaultSettings;
        Context.WorkTimeSettings.Add(settings);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAsync();

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(settings.DailyTargetMinutes, result!.DailyTargetMinutes);
    }

    [Fact]
    public async Task GetAsync_WhenNoSettingsExist_ReturnsNull()
    {
        // Arrange

        // Act
        var result = await _sut.GetAsync();

        // Assert
        Assert.Null(result);
    }

    // AddAsync

    [Fact]
    public async Task AddAsync_WhenSettingsAreValid_AddsSettingsToDatabase()
    {
        // Arrange
        var settings = WorkTimePlannerTestData.DefaultSettings;
        settings.DailyTargetMinutes = 450;
        settings.BreakRule6h = 20;
        settings.BreakRule9h = 40;
        settings.HomeOfficeLimit = 15;

        // Act
        await _sut.AddAsync(settings);

        // Assert
        var dbSettings = await Context.WorkTimeSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Id == settings.Id);
        Assert.NotNull(dbSettings);
        AssertEx.AreEqual(450, dbSettings!.DailyTargetMinutes);
        AssertEx.AreEqual(20, dbSettings.BreakRule6h);
        AssertEx.AreEqual(40, dbSettings.BreakRule9h);
        AssertEx.AreEqual(15, dbSettings.HomeOfficeLimit);
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenSettingsExist_UpdatesAllFields()
    {
        // Arrange
        var settings = WorkTimePlannerTestData.DefaultSettings;
        Context.WorkTimeSettings.Add(settings);
        await Context.SaveChangesAsync();

        settings.DailyTargetMinutes = 450;
        settings.BreakRule6h = 20;
        settings.BreakRule9h = 40;
        settings.HomeOfficeLimit = 15;

        // Act
        await _sut.UpdateAsync(settings);

        // Assert
        var dbSettings = await Context.WorkTimeSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Id == settings.Id);
        Assert.NotNull(dbSettings);
        AssertEx.AreEqual(450, dbSettings!.DailyTargetMinutes);
        AssertEx.AreEqual(20, dbSettings.BreakRule6h);
        AssertEx.AreEqual(40, dbSettings.BreakRule9h);
        AssertEx.AreEqual(15, dbSettings.HomeOfficeLimit);
    }
}
