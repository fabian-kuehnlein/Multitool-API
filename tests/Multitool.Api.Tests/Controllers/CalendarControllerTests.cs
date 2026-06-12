using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Calendar;
using Multitool.Tests.Shared;
using Multitool.Application.Models.Calendar;

namespace Multitool.Api.Tests.Controllers;

public class CalendarControllerTests
{
    private readonly Mock<ICalendarService> _serviceMock;
    private readonly CalendarController _sut;

    public CalendarControllerTests()
    {
        _serviceMock = new Mock<ICalendarService>();
        _sut = new CalendarController(_serviceMock.Object);
    }

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
    public async Task SearchEvents_WhenMatchesExist_ReturnsOkWithResults()
    {
        var results = new List<EventSearchResponseDto> { new(1, CalendarTestData.DefaultEvent.Title, null, CalendarTestData.DefaultEvent.StartDateTime, null, null) };
        _serviceMock
            .Setup(s => s.SearchCalendarEventsAsync("Meeting"))
            .ReturnsAsync(results);

        var result = await _sut.SearchEvents("Meeting");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(results);
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
}
