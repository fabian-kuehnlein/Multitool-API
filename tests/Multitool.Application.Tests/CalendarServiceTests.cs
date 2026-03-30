using FluentAssertions;
using Mapster;
using Moq;
using Multitool.Api.Tests;
using Multitool.Application.Mappings;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Tests;

public class CalendarServiceTests
{
    private readonly Mock<ICalendarRepository> _repositoryMock;
    private readonly Mock<ICalendarApiClient> _apiClientMock;
    private readonly CalendarService _sut;

    public CalendarServiceTests()
    {
       TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());

        _repositoryMock = new Mock<ICalendarRepository>();
        _apiClientMock = new Mock<ICalendarApiClient>();
        _sut = new CalendarService(_repositoryMock.Object, _apiClientMock.Object);
    }

    // GetEventsByRangeAsync

    [Fact]
    public async Task GetEventsByRangeAsync_DelegatesToRepository_AndReturnsResult()
    {
        var expected = new List<CalendarEvent> { CalendarTestData.DefaultEvent };
        _repositoryMock
            .Setup(r => r.GetEventsByRangeAsync(
                CalendarTestData.DefaultEvent.StartDateTime,
                CalendarTestData.DefaultEvent.EndDateTime!.Value,
                "1"))
            .ReturnsAsync(expected);

        var result = await _sut.GetEventsByRangeAsync(
            CalendarTestData.DefaultEvent.StartDateTime,
            CalendarTestData.DefaultEvent.EndDateTime!.Value,
            "1");

        result.Should().BeEquivalentTo(expected);
        _repositoryMock.Verify(r => r.GetEventsByRangeAsync(
            CalendarTestData.DefaultEvent.StartDateTime,
            CalendarTestData.DefaultEvent.EndDateTime!.Value,
            "1"
            ), Times.Once);
    }

    [Fact]
    public async Task GetEventsByRangeAsync_ReturnsEmptyList_WhenNoEventsExist()
    {
        _repositoryMock
            .Setup(r => r.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(new List<CalendarEvent>());

        var result = await _sut.GetEventsByRangeAsync(
            CalendarTestData.DefaultEvent.StartDateTime,
            CalendarTestData.DefaultEvent.EndDateTime!.Value,
            string.Empty);

        result.Should().BeEmpty();
    }

    // SearchCalendarEventsAsync

    [Fact]
    public async Task SearchCalendarEventsAsync_ReturnsMappedEventSearchResponses()
    {
        _repositoryMock
            .Setup(r => r.SearchCalendarEventsAsync("Meeting"))
            .ReturnsAsync(new List<CalendarEvent> { CalendarTestData.DefaultEvent });

        var result = await _sut.SearchCalendarEventsAsync("Meeting");

        result.Should().HaveCount(1);
        result[0].EventTitle.Should().Be(CalendarTestData.DefaultEvent.Title);
    }

    [Fact]
    public async Task SearchCalendarEventsAsync_ReturnsEmptyList_WhenRepositoryReturnsNoResults()
    {
        _repositoryMock
            .Setup(r => r.SearchCalendarEventsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<CalendarEvent>());

        var result = await _sut.SearchCalendarEventsAsync("nonexistent");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchCalendarEventsAsync_CallsRepository_WithUnmodifiedSearchString()
    {
        _repositoryMock
            .Setup(r => r.SearchCalendarEventsAsync("Meeting"))
            .ReturnsAsync(new List<CalendarEvent>());

        await _sut.SearchCalendarEventsAsync("Meeting");

        _repositoryMock.Verify(r => r.SearchCalendarEventsAsync("Meeting"), Times.Once);
    }

    // InsertEventAsync

    [Fact]
    public async Task InsertEventAsync_ReturnsIdFromRepository()
    {
        const long expectedId = 99L;
        _repositoryMock
            .Setup(r => r.InsertEventAsync(It.IsAny<CalendarEvent>()))
            .ReturnsAsync(expectedId);

        var result = await _sut.InsertEventAsync(CalendarTestData.DefaultCreateEvent);

        result.Should().Be(expectedId);
    }

    [Fact]
    public async Task InsertEventAsync_CallsRepository_ExactlyOnce()
    {
        _repositoryMock
            .Setup(r => r.InsertEventAsync(It.IsAny<CalendarEvent>()))
            .ReturnsAsync(1L);

        await _sut.InsertEventAsync(CalendarTestData.DefaultCreateEvent);

        _repositoryMock.Verify(r => r.InsertEventAsync(It.IsAny<CalendarEvent>()), Times.Once);
    }

    [Fact]
    public async Task InsertEventAsync_MapsCreateCalendarEvent_ToCalendarEvent()
    {
        CalendarEvent? captured = null;
        _repositoryMock
            .Setup(r => r.InsertEventAsync(It.IsAny<CalendarEvent>()))
            .Callback<CalendarEvent>(e => captured = e)
            .ReturnsAsync(1L);

        await _sut.InsertEventAsync(CalendarTestData.DefaultCreateEvent);

        captured.Should().NotBeNull();
        captured!.Title.Should().Be(CalendarTestData.DefaultCreateEvent.Title);
        captured.StartDateTime.Should().Be(CalendarTestData.DefaultCreateEvent.StartDateTime);
    }

    // UpdateEventAsync

    [Fact]
    public async Task UpdateEventAsync_DelegatesToRepository()
    {
        _repositoryMock
            .Setup(r => r.UpdateEventAsync(CalendarTestData.DefaultEvent))
            .Returns(Task.CompletedTask);

        await _sut.UpdateEventAsync(CalendarTestData.DefaultEvent);

        _repositoryMock.Verify(r => r.UpdateEventAsync(CalendarTestData.DefaultEvent), Times.Once);
    }

    // DeleteEventAsync

    [Fact]
    public async Task DeleteEventAsync_DelegatesToRepository_WithCorrectId()
    {
        _repositoryMock
            .Setup(r => r.DeleteEventAsync(CalendarTestData.DefaultEvent.Id))
            .Returns(Task.CompletedTask);

        await _sut.DeleteEventAsync(CalendarTestData.DefaultEvent.Id);

        _repositoryMock.Verify(r => r.DeleteEventAsync(CalendarTestData.DefaultEvent.Id), Times.Once);
    }

    // GetCategoriesAsync

    [Fact]
    public async Task GetCategoriesAsync_ReturnsAllCategories()
    {
        var categories = new List<Category> { CalendarTestData.DefaultCategory };
        _repositoryMock.Setup(r => r.GetCategoriesAsync()).ReturnsAsync(categories);

        var result = await _sut.GetCategoriesAsync();

        result.Should().BeEquivalentTo(categories);
    }

    // GetHolidaysAsync

    [Fact]
    public async Task GetHolidaysAsync_DelegatesToApiClient_NotRepository()
    {
        var holidays = new List<Holiday> { CalendarTestData.DefaultHoliday };
        _apiClientMock.Setup(a => a.GetHolidaysAsync("2026")).ReturnsAsync(holidays);

        var result = await _sut.GetHolidaysAsync("2026");

        result.Should().BeEquivalentTo(holidays);
        _apiClientMock.Verify(a => a.GetHolidaysAsync("2026"), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetHolidaysAsync_ReturnsEmptyList_WhenApiReturnsNoHolidays()
    {
        _apiClientMock
            .Setup(a => a.GetHolidaysAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Holiday>());

        var result = await _sut.GetHolidaysAsync("2026");

        result.Should().BeEmpty();
    }
}
