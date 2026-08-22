using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using FluentAssertions;
using Mapster;
using Moq;
using Multitool.Tests.Shared;
using Multitool.Application.Mappings;
using Multitool.Application.Models.Calendar;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Domain.Entities.Todo;

namespace Multitool.Application.Tests;

public class CalendarServiceTests
{
    private readonly Mock<ICalendarRepository> _calendarRepositoryMock;
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<ICalendarApiClient> _apiClientMock;
    private readonly CalendarService _sut;

    public CalendarServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());

        _calendarRepositoryMock = new Mock<ICalendarRepository>();
        _todoRepositoryMock = new Mock<ITodoRepository>();
        _apiClientMock = new Mock<ICalendarApiClient>();
        _sut = new CalendarService(_calendarRepositoryMock.Object, _todoRepositoryMock.Object, _apiClientMock.Object);
    }

    // GetEventsByRangeAsync

    [Fact]
    public async Task GetEventsByRangeAsync_WhenEventsAndTodosExist_ReturnsMergedList()
    {
        // Arrange
        var events = new List<CalendarEvent> { CalendarTestData.DefaultEvent };
        var todos = new List<Todo> { TodoTestData.DefaultTodo };

        _calendarRepositoryMock
            .Setup(r => r.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(events);
        _todoRepositoryMock
            .Setup(r => r.GetTodosWithDueDateInRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(todos);

        // Act
        var result = await _sut.GetEventsByRangeAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7), "");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenTodoIsIncluded_MapsTodoFieldsCorrectly()
    {
        // Arrange
        var todo = TodoTestData.DefaultTodo;
        _calendarRepositoryMock
            .Setup(r => r.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(new List<CalendarEvent>());
        _todoRepositoryMock
            .Setup(r => r.GetTodosWithDueDateInRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Todo> { todo });

        // Act
        var result = await _sut.GetEventsByRangeAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7), "");

        // Assert
        var todoEvent = result.Should().ContainSingle().Subject;
        todoEvent.Id.Should().Be($"todo-{todo.Id}");
        todoEvent.Title.Should().Be(todo.Title);
        todoEvent.Note.Should().Be(todo.Description);
        todoEvent.StartDateTime.Should().Be(todo.DueDate!.Value);
        todoEvent.IsAllDay.Should().BeTrue();
        todoEvent.IsTodo.Should().BeTrue();
        todoEvent.CategoryId.Should().Be(todo.CategoryId);
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenOnlyEventsExist_ReturnsOnlyMappedEvents()
    {
        // Arrange
        var events = new List<CalendarEvent> { CalendarTestData.DefaultEvent };
        _calendarRepositoryMock
            .Setup(r => r.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(events);
        _todoRepositoryMock
            .Setup(r => r.GetTodosWithDueDateInRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Todo>());

        // Act
        var result = await _sut.GetEventsByRangeAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7), "");

        // Assert
        result.Should().ContainSingle();
        result[0].IsTodo.Should().BeFalse();
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenTodoHasDueDate_SetsEndDateTimeToNextDay()
    {
        // Arrange
        var todo = TodoTestData.DefaultTodo;
        _calendarRepositoryMock
            .Setup(r => r.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(new List<CalendarEvent>());
        _todoRepositoryMock
            .Setup(r => r.GetTodosWithDueDateInRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Todo> { todo });

        // Act
        var result = await _sut.GetEventsByRangeAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7), "");

        // Assert
        var todoEvent = result.Should().ContainSingle().Subject;
        todoEvent.EndDateTime.Should().Be(todo.DueDate!.Value.Date.AddDays(1));
    }

    // SearchCalendarEventsAsync

    [Fact]
    public async Task SearchCalendarEventsAsync_WhenEventsExist_ReturnsMappedEventSearchResponses()
    {
        // Arrange
        _calendarRepositoryMock
            .Setup(r => r.SearchCalendarEventsAsync("Meeting"))
            .ReturnsAsync(new List<CalendarEvent> { CalendarTestData.DefaultEvent });

        // Act
        var result = await _sut.SearchCalendarEventsAsync("Meeting");

        // Assert
        result.Should().HaveCount(1);
        result[0].EventTitle.Should().Be(CalendarTestData.DefaultEvent.Title);
    }

    [Fact]
    public async Task SearchCalendarEventsAsync_WhenRepositoryReturnsNoResults_ReturnsEmptyList()
    {
        // Arrange
        _calendarRepositoryMock
            .Setup(r => r.SearchCalendarEventsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<CalendarEvent>());

        // Act
        var result = await _sut.SearchCalendarEventsAsync("nonexistent");

        // Assert
        result.Should().BeEmpty();
    }

    // InsertEventAsync

    [Fact]
    public async Task InsertEventAsync_WhenEventIsValid_ReturnsIdFromRepository()
    {
        // Arrange
        const long expectedId = 99L;
        _calendarRepositoryMock
            .Setup(r => r.CreateEventAsync(It.IsAny<CalendarEvent>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _sut.CreateEventAsync(CalendarTestData.DefaultCreateEvent);

        // Assert
        result.Should().Be(expectedId);
    }

    [Fact]
    public async Task InsertEventAsync_WhenEventIsValid_MapsToCalendarEvent()
    {
        // Arrange
        CalendarEvent? captured = null;
        _calendarRepositoryMock
            .Setup(r => r.CreateEventAsync(It.IsAny<CalendarEvent>()))
            .Callback<CalendarEvent>(e => captured = e)
            .ReturnsAsync(1L);

        // Act
        await _sut.CreateEventAsync(CalendarTestData.DefaultCreateEvent);

        // Assert
        captured.Should().NotBeNull();
        captured!.Title.Should().Be(CalendarTestData.DefaultCreateEvent.Title);
        captured.StartDateTime.Should().Be(CalendarTestData.DefaultCreateEvent.StartDateTime);
    }

    // UpdateEventAsync

    [Fact]
    public async Task UpdateEventAsync_WhenEventExists_DelegatesToRepository()
    {
        // Arrange
        var dto = CalendarTestData.DefaultUpdateEvent;
        var existingEvent = new CalendarEvent
        {
            Id = CalendarTestData.DefaultEvent.Id,
            Title = "Old Title",
            StartDateTime = DateTime.UtcNow,
            IsAllDay = false,
            CategoryId = 1
        };
        _calendarRepositoryMock
            .Setup(r => r.GetByIdAsync(existingEvent.Id))
            .ReturnsAsync(existingEvent);
        _calendarRepositoryMock
            .Setup(r => r.UpdateEventAsync(existingEvent))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateEventAsync(existingEvent.Id, dto);

        // Assert
        _calendarRepositoryMock.Verify(r => r.UpdateEventAsync(existingEvent), Times.Once);
        existingEvent.Title.Should().Be(dto.Title);
    }

    [Fact]
    public async Task UpdateEventAsync_WhenEventDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _calendarRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((CalendarEvent?)null);

        // Act
        var act = () => _sut.UpdateEventAsync(99, CalendarTestData.DefaultUpdateEvent);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _calendarRepositoryMock.Verify(r => r.UpdateEventAsync(It.IsAny<CalendarEvent>()), Times.Never);
    }

    // DeleteEventAsync

    [Fact]
    public async Task DeleteEventAsync_WhenEventExists_DelegatesToRepository_WithCorrectId()
    {
        // Arrange
        _calendarRepositoryMock
            .Setup(r => r.GetByIdAsync(CalendarTestData.DefaultEvent.Id))
            .ReturnsAsync(CalendarTestData.DefaultEvent);
        _calendarRepositoryMock
            .Setup(r => r.DeleteEventAsync(CalendarTestData.DefaultEvent.Id))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteEventAsync(CalendarTestData.DefaultEvent.Id);

        // Assert
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(CalendarTestData.DefaultEvent.Id), Times.Once);
    }

    [Fact]
    public async Task DeleteEventAsync_WhenEventDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _calendarRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((CalendarEvent?)null);

        // Act
        var act = () => _sut.DeleteEventAsync(CalendarTestData.DefaultEvent.Id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
    }

    // GetHolidaysAsync

    [Fact]
    public async Task GetHolidaysAsync_DelegatesToApiClient_NotRepository()
    {
        // Arrange
        var holidays = new List<Holiday> { CalendarTestData.DefaultHoliday };
        _apiClientMock.Setup(a => a.GetHolidaysAsync("2026")).ReturnsAsync(holidays);

        // Act
        var result = await _sut.GetHolidaysAsync("2026");

        // Assert
        result.Should().BeEquivalentTo(holidays.Adapt<List<HolidayDto>>());
        _apiClientMock.Verify(a => a.GetHolidaysAsync("2026"), Times.Once);
        _calendarRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetHolidaysAsync_WhenApiReturnsNoHolidays_ThrowsNotFoundException()
    {
        // Arrange
        _apiClientMock
            .Setup(a => a.GetHolidaysAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Holiday>());

        // Act
        var act = () => _sut.GetHolidaysAsync("2026");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // GetICalLinkAsync

    [Fact]
    public async Task GetICalLinkAsync_WhenEndDateTimeIsSet_ReturnsLinkWithAllQueryParameters()
    {
        // Arrange
        var calendarEvent = CalendarTestData.DefaultICalLinkEvent;

        // Act
        var result = await _sut.GetICalLinkAsync(calendarEvent);

        // Assert
        var query = ParseQuery(result);
        query["title"].Should().Be("Team Meeting");
        query["start"].Should().Be("2026-06-01T09:00:00.0000000Z");
        query["end"].Should().Be("2026-06-01T10:00:00.0000000Z");
        query["description"].Should().Be("Besprechung Projekt Updates");
    }

    [Fact]
    public async Task GetICalLinkAsync_WhenEndDateTimeIsNull_AddsOneHourToStartAsEnd()
    {
        // Arrange
        var calendarEvent = CalendarTestData.DefaultICalLinkEvent with { EndDateTime = null };

        // Act
        var result = await _sut.GetICalLinkAsync(calendarEvent);

        // Assert
        var query = ParseQuery(result);
        query["start"].Should().Be("2026-06-01T09:00:00.0000000Z");
        query["end"].Should().Be("2026-06-01T10:00:00.0000000Z");
    }

    [Fact]
    public async Task GetICalLinkAsync_WhenEndDateTimeEqualsStartDateTime_AddsOneHourToEnd()
    {
        // Arrange
        var calendarEvent = CalendarTestData.DefaultICalLinkEvent with
        {
            EndDateTime = CalendarTestData.DefaultICalLinkEvent.StartDateTime
        };

        // Act
        var result = await _sut.GetICalLinkAsync(calendarEvent);

        // Assert
        var query = ParseQuery(result);
        query["start"].Should().Be("2026-06-01T09:00:00.0000000Z");
        query["end"].Should().Be("2026-06-01T10:00:00.0000000Z");
    }

    [Fact]
    public async Task GetICalLinkAsync_WhenNoteIsNull_SetsEmptyDescription()
    {
        // Arrange
        var calendarEvent = CalendarTestData.DefaultICalLinkEvent with { Note = null };

        // Act
        var result = await _sut.GetICalLinkAsync(calendarEvent);

        // Assert
        var query = ParseQuery(result);
        query["description"].Should().BeEmpty();
        query["title"].Should().Be("Team Meeting");
    }

    [Fact]
    public async Task GetICalLinkAsync_WhenEndDateTimeIsBeforeStartDateTime_AddsOneHourToEnd()
    {
        // Arrange
        var calendarEvent = CalendarTestData.DefaultICalLinkEvent with
        {
            EndDateTime = CalendarTestData.DefaultICalLinkEvent.StartDateTime.AddMinutes(-30)
        };

        // Act
        var result = await _sut.GetICalLinkAsync(calendarEvent);

        // Assert
        var query = ParseQuery(result);
        query["start"].Should().Be("2026-06-01T09:00:00.0000000Z");
        query["end"].Should().Be("2026-06-01T09:30:00.0000000Z");
    }

    [Fact]
    public async Task GetICalLinkAsync_WhenDatesAreLocal_ConvertsToUtcInLink()
    {
        // Arrange
        var calendarEvent = CalendarTestData.DefaultICalLinkEvent with
        {
            StartDateTime = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Local),
            EndDateTime = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Local)
        };

        // Act
        var result = await _sut.GetICalLinkAsync(calendarEvent);

        // Assert
        var query = ParseQuery(result);
        query["start"].Should().EndWith("Z");
        query["end"].Should().EndWith("Z");
        DateTime.Parse(query["start"]!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
            .Should()
            .Be(calendarEvent.StartDateTime.ToUniversalTime());
    }

    // DeletePastEventsAsync

    [Fact]
    public async Task DeletePastEventsAsync_WhenEventIsNonRecurring_AndOldEnough_DeletesEvent()
    {
        // Arrange
        var threshold = DateTime.UtcNow.AddMonths(-3);
        var oldEvent = CalendarTestData.DefaultEvent;
        oldEvent.StartDateTime = threshold.AddDays(-10);
        oldEvent.EndDateTime = threshold.AddDays(-5);
        oldEvent.RecurrenceRule = null;

        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent> { oldEvent });

        // Act
        await _sut.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(oldEvent.Id), Times.Once);
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenEventIsNonRecurring_AndNotOldEnough_DoesNotDelete()
    {
        // Arrange
        var threshold = DateTime.UtcNow.AddMonths(-3);
        var recentEvent = CalendarTestData.DefaultEvent;
        recentEvent.StartDateTime = threshold.AddDays(5);
        recentEvent.EndDateTime = threshold.AddDays(10);
        recentEvent.RecurrenceRule = null;

        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent> { recentEvent });

        // Act
        await _sut.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenEventHasNoEndDate_UsesStartDateAsThreshold()
    {
        // Arrange
        var threshold = DateTime.UtcNow.AddMonths(-3);
        var eventWithoutEnd = CalendarTestData.DefaultEvent;
        eventWithoutEnd.StartDateTime = threshold.AddDays(-1);
        eventWithoutEnd.EndDateTime = null;
        eventWithoutEnd.RecurrenceRule = null;

        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent> { eventWithoutEnd });

        // Act
        await _sut.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(eventWithoutEnd.Id), Times.Once);
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenRecurringEvent_AndNoRecurrenceEnd_DoesNotDelete()
    {
        // Arrange
        var oldEvent = CalendarTestData.DefaultEvent;
        oldEvent.StartDateTime = DateTime.UtcNow.AddYears(-2);
        oldEvent.RecurrenceRule = "FREQ=WEEKLY";
        oldEvent.RecurrenceEnd = null;

        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent> { oldEvent });

        // Act
        await _sut.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenRecurringEvent_AndRecurrenceEndIsOldEnough_DeletesEvent()
    {
        // Arrange
        var threshold = DateTime.UtcNow.AddMonths(-3);
        var oldRecurring = CalendarTestData.DefaultEvent;
        oldRecurring.StartDateTime = threshold.AddYears(-1);
        oldRecurring.RecurrenceRule = "FREQ=WEEKLY";
        oldRecurring.RecurrenceEnd = threshold.AddDays(-1);

        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent> { oldRecurring });

        // Act
        await _sut.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(oldRecurring.Id), Times.Once);
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenRecurringEvent_AndRecurrenceEndIsNotOldEnough_DoesNotDelete()
    {
        // Arrange
        var threshold = DateTime.UtcNow.AddMonths(-3);
        var activeRecurring = CalendarTestData.DefaultEvent;
        activeRecurring.StartDateTime = threshold.AddYears(-1);
        activeRecurring.RecurrenceRule = "FREQ=WEEKLY";
        activeRecurring.RecurrenceEnd = threshold.AddDays(10);

        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent> { activeRecurring });

        // Act
        await _sut.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenNoEventsExist_DoesNotCallDelete()
    {
        // Arrange
        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent>());

        // Act
        await _sut.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
    }

    private static NameValueCollection ParseQuery(string link)
    {
        var query = link[(link.IndexOf('?') + 1)..];
        return HttpUtility.ParseQueryString(query);
    }
}
