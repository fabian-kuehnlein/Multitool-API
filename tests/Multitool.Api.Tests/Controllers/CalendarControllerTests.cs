using Moq;
using Multitool.Api.Controllers;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Calendar;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;
using Xunit;

namespace Multitool.Api.Tests.Controllers;

public class CalendarControllerTests
{
    private readonly Mock<ICalendarService> _calendarServiceMock;

    private List<CalendarEventDto> _getEventsByRangeResponse;
    private List<EventSearchResponseDto> _searchEventsResponse;
    private int _createEventResponse;
    private List<HolidayDto> _getHolidaysResponse;
    private byte[] _generateIcsResponse;

    private static readonly DateTime Start = CalendarTestData.DefaultEvent.StartDateTime;
    private static readonly DateTime End = CalendarTestData.DefaultEvent.EndDateTime!.Value;
    private static readonly int ID = CalendarTestData.DefaultEvent.Id;

    public CalendarControllerTests()
    {
        _calendarServiceMock = new Mock<ICalendarService>();

        // Default responses
        var defaultEvent = CalendarTestData.DefaultEvent;
        _getEventsByRangeResponse = [CalendarTestData.DefaultEventDto];
        _searchEventsResponse = [new EventSearchResponseDto(defaultEvent.Id, defaultEvent.Title, null, defaultEvent.StartDateTime, null, null)];
        _createEventResponse = defaultEvent.Id;
        _getHolidaysResponse = [new HolidayDto { Name = CalendarTestData.DefaultHoliday.Name, Date = CalendarTestData.DefaultHoliday.Date }];
        _generateIcsResponse = "BEGIN:VCALENDAR"u8.ToArray();;    }

    private CalendarController GetController()
    {
        _calendarServiceMock.Reset();

        _calendarServiceMock.Setup(s => s.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(_getEventsByRangeResponse);

        _calendarServiceMock.Setup(s => s.SearchCalendarEventsAsync(It.IsAny<string>()))
            .ReturnsAsync(_searchEventsResponse);

        _calendarServiceMock.Setup(s => s.CreateEventAsync(It.IsAny<CreateCalendarEventDto>()))
            .ReturnsAsync(_createEventResponse);

        _calendarServiceMock.Setup(s => s.UpdateEventAsync(It.IsAny<int>(), It.IsAny<UpdateCalendarEventDto>()))
            .Returns(Task.CompletedTask);

        _calendarServiceMock.Setup(s => s.DeleteEventAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _calendarServiceMock.Setup(s => s.GetHolidaysAsync(It.IsAny<string>()))
            .ReturnsAsync(_getHolidaysResponse);

        _calendarServiceMock.Setup(s => s.GenerateIcsFileAsync(It.IsAny<GetIcalDto>()))
            .ReturnsAsync(_generateIcsResponse);

        return new CalendarController(_calendarServiceMock.Object);
    }

    // GET api/Calendar/events

    [Fact]
    public async Task GetEventsByRange_WhenEventsExist_ReturnsOkWithEvents()
    {
        // Arrange
        const string categories = "Arbeit";
        var controller = GetController();

        // Act
        var result = await controller.GetEventsByRange(Start, End, categories);

        // Assert
        AssertEx.Ok(result, _getEventsByRangeResponse);

        _calendarServiceMock.Verify(s => s.GetEventsByRangeAsync(Start, End, categories), Times.Once);
        _calendarServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetEventsByRange_WhenCategoriesIsNull_PassesEmptyStringToService()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetEventsByRange(Start, End, null);

        // Assert
        AssertEx.Ok(result, _getEventsByRangeResponse);

        _calendarServiceMock.Verify(s => s.GetEventsByRangeAsync(Start, End, string.Empty), Times.Once);
        _calendarServiceMock.VerifyNoOtherCalls();
    }

    // GET api/Calendar/events/search

    [Fact]
    public async Task SearchEvents_WhenMatchesExist_ReturnsOkWithResults()
    {
        // Arrange
        const string searchString = "Meeting";
        var controller = GetController();

        // Act
        var result = await controller.SearchEvents(searchString);

        // Assert
        AssertEx.Ok(result, _searchEventsResponse);

        _calendarServiceMock.Verify(s => s.SearchCalendarEventsAsync(searchString), Times.Once);
        _calendarServiceMock.VerifyNoOtherCalls();
    }

    // POST api/Calendar/events

    [Fact]
    public async Task CreateEvent_WhenEventIsValid_ReturnsCreatedWithId()
    {
        // Arrange
        var newEvent = CalendarTestData.DefaultCreateEvent;
        var controller = GetController();

        // Act
        var result = await controller.CreateEvent(newEvent);

        // Assert
        AssertEx.Created(result, _createEventResponse);

        _calendarServiceMock.Verify(s => s.CreateEventAsync(newEvent), Times.Once);
        _calendarServiceMock.VerifyNoOtherCalls();
    }

    // PUT api/Calendar/events/{id}

    [Fact]
    public async Task UpdateEvent_WhenUpdateSucceeds_ReturnsNoContent()
    {
        // Arrange
        var updateEvent = CalendarTestData.DefaultUpdateEvent;
        var controller = GetController();

        // Act
        var result = await controller.UpdateEvent(ID, updateEvent);

        // Assert
        AssertEx.NoContent(result);

        _calendarServiceMock.Verify(s => s.UpdateEventAsync(ID, updateEvent), Times.Once);
        _calendarServiceMock.VerifyNoOtherCalls();
    }

    // DELETE api/Calendar/events/{id}

    [Fact]
    public async Task DeleteEvent_WhenDeletionSucceeds_ReturnsNoContent()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.DeleteEvent(ID);

        // Assert
        AssertEx.NoContent(result);

        _calendarServiceMock.Verify(s => s.DeleteEventAsync(ID), Times.Once);
        _calendarServiceMock.VerifyNoOtherCalls();
    }

    // GET api/Calendar/holidays/{year}

    [Fact]
    public async Task GetHolidays_WhenHolidaysExist_ReturnsOkWithHolidays()
    {
        // Arrange
        const string year = "2026";
        var controller = GetController();

        // Act
        var result = await controller.GetHolidays(year);

        // Assert
        AssertEx.Ok(result, _getHolidaysResponse);

        _calendarServiceMock.Verify(s => s.GetHolidaysAsync(year), Times.Once);
        _calendarServiceMock.VerifyNoOtherCalls();
    }

    // POST api/Calendar/events/ical-link

    [Fact]
    public async Task GenerateIcsFile_WhenEventIsValid_ReturnsIcsFile()
    {
        // Arrange
        var calendarEvent = CalendarTestData.DefaultIcalEvent;
        var controller = GetController();

        // Act
        var result = await controller.GenerateIcsFile(calendarEvent);

        // Assert
        AssertEx.FileResult(result, "text/calendar", "TeamMeeting.ics", _generateIcsResponse);

        _calendarServiceMock.Verify(s => s.GenerateIcsFileAsync(calendarEvent), Times.Once);
        _calendarServiceMock.VerifyNoOtherCalls();
    }
}
