using Microsoft.EntityFrameworkCore;
using Multitool.Domain.Entities.Config;
using Multitool.Domain.Entities.Settings;
using Multitool.Infrastructure.Repositories;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Infrastructure.Tests;

public class SettingRepositoryTests : RepositoryTestBase
{
    private readonly SettingRepository _settingRepository;
    private readonly int _userId;

    public SettingRepositoryTests()
    {
        _settingRepository = new SettingRepository(Context);

        var user = AuthTestData.DefaultUser;
        Context.Users.Add(user);
        Context.SaveChanges();
        _userId = user.Id;
    }

    private Setting CreateSetting(string key, string value, int? userId = null) => new()
    {
        UserId = userId ?? _userId,
        Key = key,
        Value = value,
        UpdatedAt = DateTime.Now
    };

    // GetSettingsByUserAsync

    [Fact]
    public async Task GetSettingsByUserAsync_WhenUserHasSettings_ReturnsOnlyUsersSettings()
    {
        // Arrange
        var otherUser = new User { Username = "otheruser", PasswordHash = "other-hash" };
        Context.Users.Add(otherUser);
        await Context.SaveChangesAsync();

        Context.Settings.AddRange(
            CreateSetting(SettingTestData.DailyTargetMinutesFullKey, "500"),
            CreateSetting(SettingTestData.HomeOfficeLimitMonthlyFullKey, "10"),
            CreateSetting(SettingTestData.DailyTargetMinutesFullKey, "600", otherUser.Id));
        await Context.SaveChangesAsync();

        // Act
        var result = await _settingRepository.GetSettingsByUserAsync(_userId);

        // Assert
        AssertEx.AreEqual(2, result.Count);
        Assert.Contains(result, s => s.UserId == _userId && s.Key == SettingTestData.DailyTargetMinutesFullKey && s.Value == "500");
        Assert.Contains(result, s => s.UserId == _userId && s.Key == SettingTestData.HomeOfficeLimitMonthlyFullKey && s.Value == "10");
    }

    [Fact]
    public async Task GetSettingsByUserAsync_WhenUserHasNoSettings_ReturnsEmptyList()
    {
        // Arrange
        var otherUser = new User { Username = "otheruser", PasswordHash = "other-hash" };
        Context.Users.Add(otherUser);
        await Context.SaveChangesAsync();

        Context.Settings.Add(CreateSetting(SettingTestData.DailyTargetMinutesFullKey, "500", otherUser.Id));
        await Context.SaveChangesAsync();

        // Act
        var result = await _settingRepository.GetSettingsByUserAsync(_userId);

        // Assert
        Assert.Empty(result);
    }

    // GetSettingByKeyAsync

    [Fact]
    public async Task GetSettingByKeyAsync_WhenSettingExists_ReturnsSetting()
    {
        // Arrange
        var setting = CreateSetting(SettingTestData.DailyTargetMinutesFullKey, "500");
        Context.Settings.Add(setting);
        await Context.SaveChangesAsync();

        // Act
        var result = await _settingRepository.GetSettingByKeyAsync(_userId, SettingTestData.DailyTargetMinutesFullKey);

        // Assert
        Assert.NotNull(result);
        AssertEx.AreEqual(_userId, result!.UserId);
        AssertEx.AreEqual(SettingTestData.DailyTargetMinutesFullKey, result.Key);
        AssertEx.AreEqual("500", result.Value);
    }

    [Fact]
    public async Task GetSettingByKeyAsync_WhenKeyDoesNotExist_ReturnsNull()
    {
        // Arrange
        Context.Settings.Add(CreateSetting(SettingTestData.DailyTargetMinutesFullKey, "500"));
        await Context.SaveChangesAsync();

        // Act
        var result = await _settingRepository.GetSettingByKeyAsync(_userId, "does_not_exist");

        // Assert
        Assert.Null(result);
    }

    // CreateSettingAsync

    [Fact]
    public async Task CreateSettingAsync_WhenSettingIsValid_AddsSettingToDatabase()
    {
        // Arrange
        var setting = CreateSetting(SettingTestData.DailyTargetMinutesFullKey, "600");

        // Act
        await _settingRepository.CreateSettingAsync(setting);

        // Assert
        var dbSetting = await Context.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == SettingTestData.DailyTargetMinutesFullKey);
        Assert.NotNull(dbSetting);
        AssertEx.AreEqual(_userId, dbSetting!.UserId);
        AssertEx.AreEqual("600", dbSetting.Value);
        AssertEx.AreEqual(setting.UpdatedAt, dbSetting.UpdatedAt);
    }

    // UpdateSettingAsync

    [Fact]
    public async Task UpdateSettingAsync_WhenSettingExists_UpdatesValueAndUpdatedAt()
    {
        // Arrange
        var setting = CreateSetting(SettingTestData.DailyTargetMinutesFullKey, "500");
        Context.Settings.Add(setting);
        await Context.SaveChangesAsync();

        var newUpdatedAt = new DateTime(2026, 6, 15, 10, 30, 0);
        setting.Value = "600";
        setting.UpdatedAt = newUpdatedAt;

        // Act
        await _settingRepository.UpdateSettingAsync(setting);

        // Assert
        var dbSetting = await Context.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == SettingTestData.DailyTargetMinutesFullKey);
        Assert.NotNull(dbSetting);
        AssertEx.AreEqual("600", dbSetting!.Value);
        AssertEx.AreEqual(newUpdatedAt, dbSetting.UpdatedAt);
    }
}
