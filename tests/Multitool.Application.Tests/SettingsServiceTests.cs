using Mapster;
using Moq;
using Multitool.Application.Mappings;
using Multitool.Application.Models.Settings;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Settings;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class SettingsServiceTests
{
    private readonly Mock<ISettingRepository> _settingRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;

    private List<Setting> _getSettingsResponse;
    private Setting? _getSettingByKeyResponse;

    private static readonly int USER_ID = AuthTestData.DefaultUser.Id;
    private static readonly string MODULE = SettingTestData.WorktimeModule;
    private static readonly string KEY = SettingTestData.DailyTargetMinutesKey;

    private Setting? _createdSetting;
    private Setting? _updatedSetting;

    public SettingsServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());

        _settingRepositoryMock = new Mock<ISettingRepository>();
        _userContextMock = new Mock<IUserContext>();
        _userContextMock.SetupGet(c => c.UserId).Returns(USER_ID);

        _getSettingsResponse = new List<Setting>();
        _getSettingByKeyResponse = null;
    }

    private SettingsService GetService()
    {
        _createdSetting = null;
        _updatedSetting = null;

        _settingRepositoryMock.Reset();

        _settingRepositoryMock.Setup(r => r.GetSettingsAsync())
            .ReturnsAsync(_getSettingsResponse);

        _settingRepositoryMock.Setup(r => r.GetSettingByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(_getSettingByKeyResponse);

        _settingRepositoryMock.Setup(r => r.CreateSettingAsync(It.IsAny<Setting>()))
            .Callback<Setting>(s => _createdSetting = s)
            .Returns(Task.CompletedTask);

        _settingRepositoryMock.Setup(r => r.UpdateSettingAsync(It.IsAny<Setting>()))
            .Callback<Setting>(s => _updatedSetting = s)
            .Returns(Task.CompletedTask);

        return new SettingsService(_settingRepositoryMock.Object, _userContextMock.Object);
    }

    private static Setting CreateStoredSetting(string key, string value) => new()
    {
        UserId = USER_ID,
        Key = key,
        Value = value,
        UpdatedAt = DateTime.Now
    };

    // GetSettingsAsync
    [Fact]
    public async Task GetSettingsAsync_WhenAllKeysAreStored_ReturnsStoredValuesWithoutCreating()
    {
        // Arrange
        _getSettingsResponse =
        [
            CreateStoredSetting(SettingTestData.DailyTargetMinutesFullKey, "500"),
            CreateStoredSetting(SettingTestData.HomeOfficeLimitMonthlyFullKey, "10"),
            CreateStoredSetting(SettingTestData.HomeOfficeLimitWeeklyFullKey, "2")
        ];
        var service = GetService();

        // Act
        var result = await service.GetSettingsAsync();

        // Assert
        AssertEx.AreEqual(result, new SettingsDto(new WorktimeSettingsDto(500, 10, 2)));

        _settingRepositoryMock.Verify(r => r.GetSettingsAsync(), Times.Once);
        _settingRepositoryMock.Verify(r => r.CreateSettingAsync(It.IsAny<Setting>()), Times.Never);
        _settingRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetSettingsAsync_WhenNoKeysAreStored_CreatesAllDefinedKeysWithDefaults()
    {
        // Arrange
        var service = GetService();

        // Act
        var result = await service.GetSettingsAsync();

        // Assert
        AssertEx.AreEqual(result, SettingTestData.DefaultSettingsDto);

        _settingRepositoryMock.Verify(r => r.GetSettingsAsync(), Times.Once);
        _settingRepositoryMock.Verify(r => r.CreateSettingAsync(It.Is<Setting>(s =>
            s.UserId == USER_ID &&
            s.Key == SettingTestData.DailyTargetMinutesFullKey &&
            s.Value == "480"
        )), Times.Once);
        _settingRepositoryMock.Verify(r => r.CreateSettingAsync(It.Is<Setting>(s =>
            s.UserId == USER_ID &&
            s.Key == SettingTestData.HomeOfficeLimitMonthlyFullKey &&
            s.Value == "0"
        )), Times.Once);
        _settingRepositoryMock.Verify(r => r.CreateSettingAsync(It.Is<Setting>(s =>
            s.UserId == USER_ID &&
            s.Key == SettingTestData.HomeOfficeLimitWeeklyFullKey &&
            s.Value == "0"
        )), Times.Once);
        _settingRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetSettingsAsync_WhenSomeKeysAreStored_CreatesOnlyMissingKeys()
    {
        // Arrange
        _getSettingsResponse = [CreateStoredSetting(SettingTestData.DailyTargetMinutesFullKey, "500")];
        var service = GetService();

        // Act
        var result = await service.GetSettingsAsync();

        // Assert
        AssertEx.AreEqual(result, new SettingsDto(new WorktimeSettingsDto(500, 0, 0)));

        _settingRepositoryMock.Verify(r => r.GetSettingsAsync(), Times.Once);
        _settingRepositoryMock.Verify(r => r.CreateSettingAsync(It.Is<Setting>(s =>
            s.Key == SettingTestData.DailyTargetMinutesFullKey
        )), Times.Never);
        _settingRepositoryMock.Verify(r => r.CreateSettingAsync(It.Is<Setting>(s =>
            s.UserId == USER_ID &&
            s.Key == SettingTestData.HomeOfficeLimitMonthlyFullKey &&
            s.Value == "0"
        )), Times.Once);
        _settingRepositoryMock.Verify(r => r.CreateSettingAsync(It.Is<Setting>(s =>
            s.UserId == USER_ID &&
            s.Key == SettingTestData.HomeOfficeLimitWeeklyFullKey &&
            s.Value == "0"
        )), Times.Once);
        _settingRepositoryMock.VerifyNoOtherCalls();
    }

    // UpdateSettingAsync
    [Fact]
    public async Task UpdateSettingAsync_WhenSettingExists_UpdatesValueAndUpdatedAt()
    {
        // Arrange
        var existing = SettingTestData.DefaultSetting;
        _getSettingByKeyResponse = existing;
        var dto = SettingTestData.DefaultUpdateSettingDto;
        var service = GetService();

        // Act
        await service.UpdateSettingAsync(MODULE, KEY, dto);

        // Assert
        AssertEx.AreEqual(dto.Value, _updatedSetting!.Value);
        AssertEx.CloseTo(_updatedSetting.UpdatedAt, DateTime.Now, TimeSpan.FromSeconds(5));

        _settingRepositoryMock.Verify(r => r.GetSettingByKeyAsync(SettingTestData.DailyTargetMinutesFullKey), Times.Once);
        _settingRepositoryMock.Verify(r => r.UpdateSettingAsync(It.Is<Setting>(s =>
            s.UserId == USER_ID &&
            s.Key == SettingTestData.DailyTargetMinutesFullKey &&
            s.Value == "600"
        )), Times.Once);
        _settingRepositoryMock.Verify(r => r.CreateSettingAsync(It.IsAny<Setting>()), Times.Never);
        _settingRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateSettingAsync_WhenSettingDoesNotExist_CreatesSetting()
    {
        // Arrange
        var dto = SettingTestData.DefaultUpdateSettingDto;
        var service = GetService();

        // Act
        await service.UpdateSettingAsync(MODULE, KEY, dto);

        // Assert
        AssertEx.AreEqual(_createdSetting, new Setting
        {
            UserId = USER_ID,
            Key = SettingTestData.DailyTargetMinutesFullKey,
            Value = "600",
            UpdatedAt = _createdSetting!.UpdatedAt
        });
        AssertEx.CloseTo(_createdSetting.UpdatedAt, DateTime.Now, TimeSpan.FromSeconds(5));

        _settingRepositoryMock.Verify(r => r.GetSettingByKeyAsync(SettingTestData.DailyTargetMinutesFullKey), Times.Once);
        _settingRepositoryMock.Verify(r => r.CreateSettingAsync(It.Is<Setting>(s =>
            s.UserId == USER_ID &&
            s.Key == SettingTestData.DailyTargetMinutesFullKey &&
            s.Value == "600"
        )), Times.Once);
        _settingRepositoryMock.Verify(r => r.UpdateSettingAsync(It.IsAny<Setting>()), Times.Never);
        _settingRepositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(SettingTestData.WorktimeModule, "not_defined")]
    [InlineData("not_defined", SettingTestData.DailyTargetMinutesKey)]
    public async Task UpdateSettingAsync_WhenKeyIsNotDefined_ThrowsNotFoundException(string module, string key)
    {
        // Arrange
        var dto = SettingTestData.DefaultUpdateSettingDto;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateSettingAsync(module, key, dto);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _settingRepositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("1441")]
    public async Task UpdateSettingAsync_WhenValueIsInvalid_ThrowsArgumentException(string value)
    {
        // Arrange
        var dto = new UpdateSettingDto(value);
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateSettingAsync(MODULE, KEY, dto);

        // Assert
        await AssertEx.Throws<ArgumentException>(act);

        _settingRepositoryMock.VerifyNoOtherCalls();
    }
}
