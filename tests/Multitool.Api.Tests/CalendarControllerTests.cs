using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Calendar;
using Multitool.Tests.Shared;

namespace Multitool.Api.Tests;

public class CalendarControllerTests
{
    private readonly Mock<ICalendarService> _serviceMock;
    private readonly CalendarController _sut;

    public CalendarControllerTests()
    {
        _serviceMock = new Mock<ICalendarService>();
        _sut = new CalendarController(_serviceMock.Object);
    }

    // GET /api/calendar/events

    [Fact]
    public async Task GetEventsByRange_WhenEventsExist_ReturnsOkWithEvents()
    {
        var events = new List<CalendarEvent> { CalendarTestData.DefaultEvent };
        _serviceMock
            .Setup(s => s.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(events);

        var result = await _sut.GetEventsByRange(
            CalendarTestData.DefaultEvent.StartDateTime,
            CalendarTestData.DefaultEvent.EndDateTime!.Value,
            null);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(events);
    }

    [Fact]
    public async Task GetEventsByRange_WhenCategoriesIsNull_PassesEmptyStringToService()
    {
        _serviceMock
            .Setup(s => s.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), string.Empty))
            .ReturnsAsync(new List<CalendarEvent>());

        await _sut.GetEventsByRange(
            CalendarTestData.DefaultEvent.StartDateTime,
            CalendarTestData.DefaultEvent.EndDateTime!.Value,
            null);

        _serviceMock.Verify(s => s.GetEventsByRangeAsync(
            CalendarTestData.DefaultEvent.StartDateTime,
            CalendarTestData.DefaultEvent.EndDateTime!.Value,
            string.Empty), Times.Once);
    }

    [Fact]
    public async Task GetEventsByRange_WhenCategoriesAreProvided_PassesThemToService()
    {
        const string categories = "1,2,3";
        _serviceMock
            .Setup(s => s.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), categories))
            .ReturnsAsync(new List<CalendarEvent>());

        await _sut.GetEventsByRange(
            CalendarTestData.DefaultEvent.StartDateTime,
            CalendarTestData.DefaultEvent.EndDateTime!.Value,
            categories);

        _serviceMock.Verify(s => s.GetEventsByRangeAsync(
            CalendarTestData.DefaultEvent.StartDateTime,
            CalendarTestData.DefaultEvent.EndDateTime!.Value,
            categories), Times.Once);
    }

    [Fact]
    public async Task GetEventsByRange_WhenNoEventsExist_ReturnsEmptyList()
    {
        _serviceMock
            .Setup(s => s.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(new List<CalendarEvent>());

        var result = await _sut.GetEventsByRange(
            CalendarTestData.DefaultEvent.StartDateTime,
            CalendarTestData.DefaultEvent.EndDateTime!.Value,
            null);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new List<CalendarEvent>());
    }

    [Fact]
    public async Task GetEventsByRange_ForwardsAllParametersCorrectly()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 1, 2);
        var categories = "5";

        _serviceMock
            .Setup(s => s.GetEventsByRangeAsync(start, end, categories))
            .ReturnsAsync(new List<CalendarEvent>());

        await _sut.GetEventsByRange(start, end, categories);

        _serviceMock.Verify(s => s.GetEventsByRangeAsync(start, end, categories), Times.Once);
    }

    // GET /api/calendar/events/search

    [Fact]
    public async Task SearchEvents_WhenMatchesExist_ReturnsOkWithResults()
    {
        var results = new List<EventSearchResponse> { new() { EventTitle = CalendarTestData.DefaultEvent.Title, StartDateTime = CalendarTestData.DefaultEvent.StartDateTime } };
        _serviceMock
            .Setup(s => s.SearchCalendarEventsAsync("Meeting"))
            .ReturnsAsync(results);

        var result = await _sut.SearchEvents("Meeting");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(results);
    }

    [Fact]
    public async Task SearchEvents_WhenNoMatchesExist_ReturnsOkWithEmptyList()
    {
        _serviceMock
            .Setup(s => s.SearchCalendarEventsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<EventSearchResponse>());

        var result = await _sut.SearchEvents("nonexistent");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new List<EventSearchResponse>());
    }

    [Fact]
    public async Task SearchEvents_ForwardsSearchStringToService()
    {
        const string search = "Test";

        _serviceMock
            .Setup(s => s.SearchCalendarEventsAsync(search))
            .ReturnsAsync(new List<EventSearchResponse>());

        await _sut.SearchEvents(search);

        _serviceMock.Verify(s => s.SearchCalendarEventsAsync(search), Times.Once);
    }

    // POST /api/calendar/events

    [Fact]
    public async Task InsertEvent_WhenEventIsValid_ReturnsOkWithId()
    {
        const long expectedId = 2;
        _serviceMock
            .Setup(s => s.InsertEventAsync(CalendarTestData.DefaultCreateEvent))
            .ReturnsAsync(expectedId);

        var result = await _sut.InsertEvent(CalendarTestData.DefaultCreateEvent);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(expectedId);
    }

    [Fact]
    public async Task InsertEvent_ForwardsEventToService()
    {
        _serviceMock
            .Setup(s => s.InsertEventAsync(CalendarTestData.DefaultCreateEvent))
            .ReturnsAsync(1L);

        await _sut.InsertEvent(CalendarTestData.DefaultCreateEvent);

        _serviceMock.Verify(s => s.InsertEventAsync(CalendarTestData.DefaultCreateEvent), Times.Once);
    }

    // PUT /api/calendar/events

    [Fact]
    public async Task UpdateEvent_WhenUpdateSucceeds_ReturnsNoContent()
    {
        _serviceMock
            .Setup(s => s.UpdateEventAsync(CalendarTestData.DefaultEvent))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateEvent(CalendarTestData.DefaultEvent);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateEvent_ForwardsEventToService()
    {
        _serviceMock
            .Setup(s => s.UpdateEventAsync(It.IsAny<CalendarEvent>()))
            .Returns(Task.CompletedTask);

        await _sut.UpdateEvent(CalendarTestData.DefaultEvent);

        _serviceMock.Verify(s => s.UpdateEventAsync(CalendarTestData.DefaultEvent), Times.Once);
    }

    // DELETE /api/calendar/events/{id}

    [Fact]
    public async Task DeleteEvent_WhenDeletionSucceeds_ReturnsNoContent()
    {
        _serviceMock
            .Setup(s => s.DeleteEventAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.DeleteEvent(CalendarTestData.DefaultEvent.Id);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteEvent_ForwardsIdToService()
    {
        _serviceMock
            .Setup(s => s.DeleteEventAsync(CalendarTestData.DefaultEvent.Id))
            .Returns(Task.CompletedTask);

        await _sut.DeleteEvent(CalendarTestData.DefaultEvent.Id);

        _serviceMock.Verify(s => s.DeleteEventAsync(CalendarTestData.DefaultEvent.Id), Times.Once);
    }

    // GET /api/calendar/holidays/{year}

    [Fact]
    public async Task GetHolidays_WhenHolidaysExist_ReturnsOkWithHolidays()
    {
        var holidays = new List<Holiday> { CalendarTestData.DefaultHoliday };
        _serviceMock.Setup(s => s.GetHolidaysAsync("2026")).ReturnsAsync(holidays);

        var result = await _sut.GetHolidays("2026");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(holidays);
    }

    [Fact]
    public async Task GetHolidays_ForwardsYearToService()
    {
        _serviceMock
            .Setup(s => s.GetHolidaysAsync("2026"))
            .ReturnsAsync(new List<Holiday>());

        await _sut.GetHolidays("2026");

        _serviceMock.Verify(s => s.GetHolidaysAsync("2026"), Times.Once);
    }

    [Fact]
    public async Task GetHolidays_WhenNoHolidaysExist_ReturnsOkWithEmptyList()
    {
        _serviceMock
            .Setup(s => s.GetHolidaysAsync("2026"))
            .ReturnsAsync(new List<Holiday>());

        var result = await _sut.GetHolidays("2026");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new List<Holiday>());
    }
}
