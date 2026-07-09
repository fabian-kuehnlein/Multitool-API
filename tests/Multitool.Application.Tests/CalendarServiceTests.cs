using FluentAssertions;
using Mapster;
using Moq;
using Multitool.Tests.Shared;
using Multitool.Application.Mappings;
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
        var events = new List<CalendarEvent> { CalendarTestData.DefaultEvent };
        var todos = new List<Todo> { TodoTestData.DefaultTodo };

        _calendarRepositoryMock
            .Setup(r => r.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(events);
        _todoRepositoryMock
            .Setup(r => r.GetTodosWithDueDateInRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(todos);

        var result = await _sut.GetEventsByRangeAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7), "");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenTodoIsIncluded_MapsTodoFieldsCorrectly()
    {
        var todo = TodoTestData.DefaultTodo;
        _calendarRepositoryMock
            .Setup(r => r.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(new List<CalendarEvent>());
        _todoRepositoryMock
            .Setup(r => r.GetTodosWithDueDateInRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Todo> { todo });

        var result = await _sut.GetEventsByRangeAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7), "");

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
        var events = new List<CalendarEvent> { CalendarTestData.DefaultEvent };
        _calendarRepositoryMock
            .Setup(r => r.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(events);
        _todoRepositoryMock
            .Setup(r => r.GetTodosWithDueDateInRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Todo>());

        var result = await _sut.GetEventsByRangeAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7), "");

        result.Should().ContainSingle();
        result[0].IsTodo.Should().BeFalse();
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenTodoEndDateTimeIsSetToNextDay()
    {
        var todo = TodoTestData.DefaultTodo;
        _calendarRepositoryMock
            .Setup(r => r.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(new List<CalendarEvent>());
        _todoRepositoryMock
            .Setup(r => r.GetTodosWithDueDateInRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Todo> { todo });

        var result = await _sut.GetEventsByRangeAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7), "");

        var todoEvent = result.Should().ContainSingle().Subject;
        todoEvent.EndDateTime.Should().Be(todo.DueDate!.Value.Date.AddDays(1));
    }

    // SearchCalendarEventsAsync

    [Fact]
    public async Task SearchCalendarEventsAsync_ReturnsMappedEventSearchResponses()
    {
        _calendarRepositoryMock
            .Setup(r => r.SearchCalendarEventsAsync("Meeting"))
            .ReturnsAsync(new List<CalendarEvent> { CalendarTestData.DefaultEvent });

        var result = await _sut.SearchCalendarEventsAsync("Meeting");

        result.Should().HaveCount(1);
        result[0].EventTitle.Should().Be(CalendarTestData.DefaultEvent.Title);
    }

    [Fact]
    public async Task SearchCalendarEventsAsync_ReturnsEmptyList_WhenRepositoryReturnsNoResults()
    {
        _calendarRepositoryMock
            .Setup(r => r.SearchCalendarEventsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<CalendarEvent>());

        var result = await _sut.SearchCalendarEventsAsync("nonexistent");

        result.Should().BeEmpty();
    }

    // InsertEventAsync

    [Fact]
    public async Task InsertEventAsync_ReturnsIdFromRepository()
    {
        const long expectedId = 99L;
        _calendarRepositoryMock
            .Setup(r => r.InsertEventAsync(It.IsAny<CalendarEvent>()))
            .ReturnsAsync(expectedId);

        var result = await _sut.InsertEventAsync(CalendarTestData.DefaultCreateEvent);

        result.Should().Be(expectedId);
    }

    [Fact]
    public async Task InsertEventAsync_MapsCreateCalendarEvent_ToCalendarEvent()
    {
        CalendarEvent? captured = null;
        _calendarRepositoryMock
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
    public async Task UpdateEventAsync_WhenEventExists_DelegatesToRepository()
    {
        var existingEvent = new CalendarEvent
        {
            Id = CalendarTestData.DefaultEvent.Id,
            Title = "Old Title",
            StartDateTime = DateTime.UtcNow,
            IsAllDay = false,
            CategoryId = 1
        };
        _calendarRepositoryMock
            .Setup(r => r.GetByIdAsync(CalendarTestData.DefaultEvent.Id))
            .ReturnsAsync(existingEvent);
        _calendarRepositoryMock
            .Setup(r => r.UpdateEventAsync(existingEvent))
            .Returns(Task.CompletedTask);

        await _sut.UpdateEventAsync(CalendarTestData.DefaultEvent);

        _calendarRepositoryMock.Verify(r => r.UpdateEventAsync(existingEvent), Times.Once);
        existingEvent.Title.Should().Be(CalendarTestData.DefaultEvent.Title);
    }

    [Fact]
    public async Task UpdateEventAsync_WhenEventDoesNotExist_ThrowsNotFoundException()
    {
        _calendarRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((CalendarEvent?)null);

        var act = () => _sut.UpdateEventAsync(CalendarTestData.DefaultEvent);

        await act.Should().ThrowAsync<NotFoundException>();
        _calendarRepositoryMock.Verify(r => r.UpdateEventAsync(It.IsAny<CalendarEvent>()), Times.Never);
    }

    // DeleteEventAsync

    [Fact]
    public async Task DeleteEventAsync_WhenEventExists_DelegatesToRepository_WithCorrectId()
    {
        _calendarRepositoryMock
            .Setup(r => r.GetByIdAsync(CalendarTestData.DefaultEvent.Id))
            .ReturnsAsync(CalendarTestData.DefaultEvent);
        _calendarRepositoryMock
            .Setup(r => r.DeleteEventAsync(CalendarTestData.DefaultEvent.Id))
            .Returns(Task.CompletedTask);

        await _sut.DeleteEventAsync(CalendarTestData.DefaultEvent.Id);

        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(CalendarTestData.DefaultEvent.Id), Times.Once);
    }

    [Fact]
    public async Task DeleteEventAsync_WhenEventDoesNotExist_ThrowsNotFoundException()
    {
        _calendarRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((CalendarEvent?)null);

        var act = () => _sut.DeleteEventAsync(CalendarTestData.DefaultEvent.Id);

        await act.Should().ThrowAsync<NotFoundException>();
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
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
        _calendarRepositoryMock.VerifyNoOtherCalls();
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

    // DeletePastEventsAsync

    [Fact]
    public async Task DeletePastEventsAsync_WhenEventIsNonRecurring_AndOldEnough_DeletesEvent()
    {
        var threshold = DateTime.UtcNow.AddMonths(-3);
        var oldEvent = CalendarTestData.DefaultEvent;
        oldEvent.StartDateTime = threshold.AddDays(-10);
        oldEvent.EndDateTime = threshold.AddDays(-5);
        oldEvent.RecurrenceRule = null;

        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent> { oldEvent });

        await _sut.DeletePastEventsAsync(3);

        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(oldEvent.Id), Times.Once);
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenEventIsNonRecurring_AndNotOldEnough_DoesNotDelete()
    {
        var threshold = DateTime.UtcNow.AddMonths(-3);
        var recentEvent = CalendarTestData.DefaultEvent;
        recentEvent.StartDateTime = threshold.AddDays(5);
        recentEvent.EndDateTime = threshold.AddDays(10);
        recentEvent.RecurrenceRule = null;

        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent> { recentEvent });

        await _sut.DeletePastEventsAsync(3);

        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenEventHasNoEndDate_UsesStartDateAsThreshold()
    {
        var threshold = DateTime.UtcNow.AddMonths(-3);
        var eventWithoutEnd = CalendarTestData.DefaultEvent;
        eventWithoutEnd.StartDateTime = threshold.AddDays(-1);
        eventWithoutEnd.EndDateTime = null;
        eventWithoutEnd.RecurrenceRule = null;

        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent> { eventWithoutEnd });

        await _sut.DeletePastEventsAsync(3);

        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(eventWithoutEnd.Id), Times.Once);
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenRecurringEvent_AndNoRecurrenceEnd_DoesNotDelete()
    {
        var oldEvent = CalendarTestData.DefaultEvent;
        oldEvent.StartDateTime = DateTime.UtcNow.AddYears(-2);
        oldEvent.RecurrenceRule = "FREQ=WEEKLY";
        oldEvent.RecurrenceEnd = null;

        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent> { oldEvent });

        await _sut.DeletePastEventsAsync(3);

        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenRecurringEvent_AndRecurrenceEndIsOldEnough_DeletesEvent()
    {
        var threshold = DateTime.UtcNow.AddMonths(-3);
        var oldRecurring = CalendarTestData.DefaultEvent;
        oldRecurring.StartDateTime = threshold.AddYears(-1);
        oldRecurring.RecurrenceRule = "FREQ=WEEKLY";
        oldRecurring.RecurrenceEnd = threshold.AddDays(-1);

        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent> { oldRecurring });

        await _sut.DeletePastEventsAsync(3);

        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(oldRecurring.Id), Times.Once);
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenRecurringEvent_AndRecurrenceEndIsNotOldEnough_DoesNotDelete()
    {
        var threshold = DateTime.UtcNow.AddMonths(-3);
        var activeRecurring = CalendarTestData.DefaultEvent;
        activeRecurring.StartDateTime = threshold.AddYears(-1);
        activeRecurring.RecurrenceRule = "FREQ=WEEKLY";
        activeRecurring.RecurrenceEnd = threshold.AddDays(10);

        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent> { activeRecurring });

        await _sut.DeletePastEventsAsync(3);

        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenNoEventsExist_DoesNotCallDelete()
    {
        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CalendarEvent>());

        await _sut.DeletePastEventsAsync(3);

        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
    }
}
