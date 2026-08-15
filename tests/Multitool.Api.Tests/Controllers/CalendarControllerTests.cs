using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Application.Models.Calendar;

namespace Multitool.Api.Tests.Controllers;

public class CalendarControllerTests
{
    private readonly Mock<ICalendarService> _serviceMock;
    private readonly CalendarController _sut;

    private static readonly DateTime Start = CalendarTestData.DefaultEvent.StartDateTime;
    private static readonly DateTime End = CalendarTestData.DefaultEvent.EndDateTime!.Value;

    public CalendarControllerTests()
    {
        _serviceMock = new Mock<ICalendarService>();
        _sut = new CalendarController(_serviceMock.Object);
    }

    // GET api/Calendar/events

    [Fact]
    public async Task GetEventsByRange_WhenEventsExist_ReturnsOkWithEvents()
    {
        // Arrange
        var events = new List<CalendarEventDto> { CalendarTestData.DefaultEventDto };
        _serviceMock
            .Setup(s => s.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(events);

        // Act
        var result = await _sut.GetEventsByRange(Start, End, null);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(events);
    }

    [Fact]
    public async Task GetEventsByRange_WhenCategoriesIsNull_PassesEmptyStringToService()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), string.Empty))
            .ReturnsAsync(new List<CalendarEventDto>());

        // Act
        await _sut.GetEventsByRange(Start, End, null);

        // Assert
        _serviceMock.Verify(s => s.GetEventsByRangeAsync(Start, End, string.Empty), Times.Once);
    }

    // GET api/Calendar/events/search

    [Fact]
    public async Task SearchEvents_WhenMatchesExist_ReturnsOkWithResults()
    {
        // Arrange
        var results = new List<EventSearchResponseDto> { new(1, CalendarTestData.DefaultEvent.Title, null, Start, null, null) };
        _serviceMock
            .Setup(s => s.SearchCalendarEventsAsync("Meeting"))
            .ReturnsAsync(results);

        // Act
        var result = await _sut.SearchEvents("Meeting");

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(results);
    }

    // POST api/Calendar/events

    [Fact]
    public async Task InsertEvent_WhenEventIsValid_ReturnsOkWithId()
    {
        // Arrange
        var createEvent = CalendarTestData.DefaultCreateEvent;
        const long expectedId = 2;
        _serviceMock
            .Setup(s => s.InsertEventAsync(createEvent))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _sut.InsertEvent(createEvent);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(expectedId);
    }

    // PUT api/Calendar/events/{id}

    [Fact]
    public async Task UpdateEvent_WhenUpdateSucceeds_ReturnsNoContent()
    {
        // Arrange
        var dto = CalendarTestData.DefaultUpdateEvent;
        const int eventId = 1;
        _serviceMock
            .Setup(s => s.UpdateEventAsync(eventId, dto))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateEvent(eventId, dto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    // DELETE api/Calendar/events/{id}

    [Fact]
    public async Task DeleteEvent_WhenDeletionSucceeds_ReturnsNoContent()
    {
        // Arrange
        var eventId = CalendarTestData.DefaultEvent.Id;
        _serviceMock
            .Setup(s => s.DeleteEventAsync(eventId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteEvent(eventId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    // GET api/Calendar/holidays/{year}

    [Fact]
    public async Task GetHolidays_WhenHolidaysExist_ReturnsOkWithHolidays()
    {
        // Arrange
        var holidays = new List<HolidayDto> { new() { Name = "Neujahr", Date = new DateTime(2026, 1, 1) } };
        const string year = "2026";
        _serviceMock.Setup(s => s.GetHolidaysAsync(year)).ReturnsAsync(holidays);

        // Act
        var result = await _sut.GetHolidays(year);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(holidays);
    }

    // POST api/Calendar/events/ical-link

    [Fact]
    public async Task GetICalLink_WhenEventIsValid_ReturnsOkWithLink()
    {
        // Arrange
        var calendarEvent = CalendarTestData.DefaultICalLinkEvent;
        const string expectedLink = "https://api.getcal.link/event.ics?title=Team+Meeting";
        _serviceMock
            .Setup(s => s.GetICalLinkAsync(calendarEvent))
            .ReturnsAsync(expectedLink);

        // Act
        var result = await _sut.GetICalLink(calendarEvent);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(expectedLink);
    }
}
