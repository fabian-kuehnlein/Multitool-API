using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using Mapster;
using Moq;
using Multitool.Application.Mappings;
using Multitool.Application.Models.Calendar;
using Multitool.Application.Services;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Entities.Category;
using Multitool.Domain.Entities.Todo;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Tests.Shared;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Application.Tests;

public class CalendarServiceTests
{
    private readonly Mock<ICalendarRepository> _calendarRepositoryMock;
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICalendarApiClient> _apiClientMock;

    private List<CalendarEvent> _getEventsByRangeResponse;
    private List<Todo> _getTodosInRangeResponse;
    private List<CalendarEvent> _searchCalendarEventsResponse;
    private long _createEventResponse;
    private CalendarEvent? _getByIdResponse;
    private List<Holiday> _getHolidaysResponse;
    private List<CalendarEvent> _getEventsOlderThanResponse;
    private Category? _categoryGetByIdResponse;

    private static readonly int ID = CalendarTestData.DefaultEvent.Id;

    private CalendarEvent? _createdEvent;
    private CalendarEvent? _updatedEvent;

    public CalendarServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());

        _calendarRepositoryMock = new Mock<ICalendarRepository>();
        _todoRepositoryMock = new Mock<ITodoRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _apiClientMock = new Mock<ICalendarApiClient>();

        _getEventsByRangeResponse = new List<CalendarEvent>();
        _getTodosInRangeResponse = new List<Todo>();
        _searchCalendarEventsResponse = new List<CalendarEvent>();
        _createEventResponse = 1L;
        _getByIdResponse = CalendarTestData.DefaultEvent;
        _getHolidaysResponse = new List<Holiday>();
        _getEventsOlderThanResponse = new List<CalendarEvent>();
        _categoryGetByIdResponse = CategoryTestData.DefaultCategory;
    }

    private CalendarService GetService()
    {
        _createdEvent = null;
        _updatedEvent = null;

        _calendarRepositoryMock.Reset();
        _todoRepositoryMock.Reset();
        _categoryRepositoryMock.Reset();
        _apiClientMock.Reset();

        _calendarRepositoryMock.Setup(r => r.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(_getEventsByRangeResponse);

        _todoRepositoryMock.Setup(r => r.GetTodosWithDueDateInRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(_getTodosInRangeResponse);

        _calendarRepositoryMock.Setup(r => r.SearchCalendarEventsAsync(It.IsAny<string>()))
            .ReturnsAsync(_searchCalendarEventsResponse);

        _calendarRepositoryMock.Setup(r => r.CreateEventAsync(It.IsAny<CalendarEvent>()))
            .Callback<CalendarEvent>(e => _createdEvent = e)
            .ReturnsAsync(_createEventResponse);

        _calendarRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(_getByIdResponse);

        _calendarRepositoryMock.Setup(r => r.UpdateEventAsync(It.IsAny<CalendarEvent>()))
            .Callback<CalendarEvent>(e => _updatedEvent = e)
            .Returns(Task.CompletedTask);

        _calendarRepositoryMock.Setup(r => r.DeleteEventAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _calendarRepositoryMock.Setup(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(_getEventsOlderThanResponse);

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(_categoryGetByIdResponse);

        _apiClientMock.Setup(a => a.GetHolidaysAsync(It.IsAny<string>()))
            .ReturnsAsync(_getHolidaysResponse);

        return new CalendarService(
            _calendarRepositoryMock.Object,
            _todoRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _apiClientMock.Object);
    }

    // GetEventsByRangeAsync
    [Fact]
    public async Task GetEventsByRangeAsync_WhenEventsAndTodosExist_ReturnsMergedList()
    {
        // Arrange
        var events = new List<CalendarEvent> { CalendarTestData.DefaultEvent };
        var todos = new List<Todo> { TodoTestData.DefaultTodo };

        var expected = events.Adapt<List<CalendarEventDto>>();
        expected.Add(new CalendarEventDto
        {
            Id = $"todo-{TodoTestData.DefaultTodo.Id}",
            Title = TodoTestData.DefaultTodo.Title,
            Note = TodoTestData.DefaultTodo.Description,
            StartDateTime = TodoTestData.DefaultTodo.DueDate!.Value,
            EndDateTime = TodoTestData.DefaultTodo.DueDate.Value.Date.AddDays(1),
            IsAllDay = true,
            CategoryId = TodoTestData.DefaultTodo.CategoryId,
            RecurrenceRule = null,
            RecurrenceEnd = null,
            IsTodo = true
        });

        _getEventsByRangeResponse = events;
        _getTodosInRangeResponse = todos;
        var service = GetService();

        // Act
        var result = await service.GetEventsByRangeAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7), "");

        // Assert
        AssertEx.AreEqual(result, expected);

        _calendarRepositoryMock.Verify(r => r.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), ""), Times.Once);
        _calendarRepositoryMock.VerifyNoOtherCalls();

        _todoRepositoryMock.Verify(r => r.GetTodosWithDueDateInRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        _todoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenOnlyEventsExist_ReturnsOnlyMappedEvents()
    {
        // Arrange
        var events = new List<CalendarEvent> { CalendarTestData.DefaultEvent };
        _getEventsByRangeResponse = events;
        var service = GetService();

        // Act
        var result = await service.GetEventsByRangeAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7), "");

        // Assert
        AssertEx.AreEqual(result, events.Adapt<List<CalendarEventDto>>());

        _calendarRepositoryMock.Verify(r => r.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), ""), Times.Once);
        _calendarRepositoryMock.VerifyNoOtherCalls();

        _todoRepositoryMock.Verify(r => r.GetTodosWithDueDateInRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        _todoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetEventsByRangeAsync_WhenTodoIsIncluded_MapsTodoFieldsCorrectly()
    {
        // Arrange
        var todo = TodoTestData.DefaultTodo;
        _getTodosInRangeResponse = new List<Todo> { todo };
        var service = GetService();

        // Act
        var result = await service.GetEventsByRangeAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7), "");

        // Assert
        AssertEx.AreEqual(result, new List<CalendarEventDto>
        {
            new()
            {
                Id = $"todo-{todo.Id}",
                Title = todo.Title,
                Note = todo.Description,
                StartDateTime = todo.DueDate!.Value,
                EndDateTime = todo.DueDate.Value.Date.AddDays(1),
                IsAllDay = true,
                CategoryId = todo.CategoryId,
                RecurrenceRule = null,
                RecurrenceEnd = null,
                IsTodo = true
            }
        });

        _calendarRepositoryMock.Verify(r => r.GetEventsByRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), ""), Times.Once);
        _calendarRepositoryMock.VerifyNoOtherCalls();
        
        _todoRepositoryMock.Verify(r => r.GetTodosWithDueDateInRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        _todoRepositoryMock.VerifyNoOtherCalls();
    }

    // SearchCalendarEventsAsync
    [Fact]
    public async Task SearchCalendarEventsAsync_WhenEventsExist_ReturnsMappedEventSearchResponses()
    {
        // Arrange
        var events = new List<CalendarEvent> { CalendarTestData.DefaultEvent };
        _searchCalendarEventsResponse = events;
        var service = GetService();

        // Act
        var result = await service.SearchCalendarEventsAsync("Meeting");

        // Assert
        AssertEx.AreEqual(result, events.Adapt<List<EventSearchResponseDto>>());

        _calendarRepositoryMock.Verify(r => r.SearchCalendarEventsAsync("Meeting"), Times.Once);
        _calendarRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SearchCalendarEventsAsync_WhenRepositoryReturnsNoResults_ReturnsEmptyList()
    {
        // Arrange
        _searchCalendarEventsResponse = new List<CalendarEvent>();
        var service = GetService();

        // Act
        var result = await service.SearchCalendarEventsAsync("nonexistent");

        // Assert
        AssertEx.AreEqual(result, new List<EventSearchResponseDto>());

        _calendarRepositoryMock.Verify(r => r.SearchCalendarEventsAsync("nonexistent"), Times.Once);
        _calendarRepositoryMock.VerifyNoOtherCalls();
    }

    // CreateEventAsync
    [Fact]
    public async Task CreateEventAsync_WhenDtoIsValid_CallsRepositoryAdd()
    {
        // Arrange
        const long expectedId = 99L;
        var dto = CalendarTestData.DefaultCreateEvent;
        _createEventResponse = expectedId;
        var service = GetService();

        // Act
        var result = await service.CreateEventAsync(dto);

        // Assert
        AssertEx.AreEqual(result, expectedId);
        AssertEx.AreEqual(_createdEvent, new CalendarEvent
        {
            Title = dto.Title,
            Note = dto.Note,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime,
            IsAllDay = dto.IsAllDay,
            CategoryId = dto.CategoryId,
            RecurrenceRule = dto.RecurrenceRule,
            RecurrenceEnd = dto.RecurrenceEnd
        });

        _calendarRepositoryMock.Verify(r => r.CreateEventAsync(It.Is<CalendarEvent>(e =>
            e.Title == dto.Title &&
            e.StartDateTime == dto.StartDateTime &&
            e.EndDateTime == dto.EndDateTime &&
            e.CategoryId == dto.CategoryId
        )), Times.Once);
        _calendarRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(dto.CategoryId), Times.Once);
        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateEventAsync_WhenCategoryDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _categoryGetByIdResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.CreateEventAsync(CalendarTestData.DefaultCreateEvent);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _calendarRepositoryMock.Verify(r => r.CreateEventAsync(It.IsAny<CalendarEvent>()), Times.Never);
        _calendarRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(CalendarTestData.DefaultCreateEvent.CategoryId), Times.Once);
        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateEventAsync_WhenCategoryIsNotAvailableForCalendarModule_ThrowsCategoryNotAvailableForModuleException()
    {
        // Arrange
        var category = CategoryTestData.DefaultCategory;
        category.ApplicableModules = [Multitool.Domain.Enums.AppModule.Todo];
        _categoryGetByIdResponse = category;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.CreateEventAsync(CalendarTestData.DefaultCreateEvent);

        // Assert
        await AssertEx.Throws<CategoryNotAvailableForModuleException>(act);

        _calendarRepositoryMock.Verify(r => r.CreateEventAsync(It.IsAny<CalendarEvent>()), Times.Never);
        _calendarRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(CalendarTestData.DefaultCreateEvent.CategoryId), Times.Once);
        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    // UpdateEventAsync
    [Fact]
    public async Task UpdateEventAsync_WhenEventExists_UpdatesAllFields()
    {
        // Arrange
        var existing = CalendarTestData.DefaultEvent;
        var dto = CalendarTestData.DefaultUpdateEvent;

        _getByIdResponse = existing;
        var service = GetService();

        // Act
        await service.UpdateEventAsync(existing.Id, dto);

        // Assert
        AssertEx.AreEqual(_updatedEvent, new CalendarEvent
        {
            Id = existing.Id,
            Title = dto.Title,
            Note = dto.Note,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime,
            IsAllDay = dto.IsAllDay,
            CategoryId = dto.CategoryId,
            RecurrenceRule = dto.RecurrenceRule,
            RecurrenceEnd = dto.RecurrenceEnd
        });

        _calendarRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _calendarRepositoryMock.Verify(r => r.UpdateEventAsync(It.Is<CalendarEvent>(e =>
            e.Id == ID &&
            e.Title == dto.Title &&
            e.CategoryId == dto.CategoryId
        )), Times.Once);
        _calendarRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(dto.CategoryId), Times.Once);
        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateEventAsync_WhenEventDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getByIdResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateEventAsync(99, CalendarTestData.DefaultUpdateEvent);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _calendarRepositoryMock.Verify(r => r.GetByIdAsync(99), Times.Once);
        _calendarRepositoryMock.Verify(r => r.UpdateEventAsync(It.IsAny<CalendarEvent>()), Times.Never);
        _calendarRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateEventAsync_WhenCategoryIsNotAvailableForCalendarModule_ThrowsCategoryNotAvailableForModuleException()
    {
        // Arrange
        var category = CategoryTestData.DefaultCategory;
        category.ApplicableModules = [Multitool.Domain.Enums.AppModule.Todo];
        _categoryGetByIdResponse = category;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.UpdateEventAsync(ID, CalendarTestData.DefaultUpdateEvent);

        // Assert
        await AssertEx.Throws<CategoryNotAvailableForModuleException>(act);

        _calendarRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _calendarRepositoryMock.Verify(r => r.UpdateEventAsync(It.IsAny<CalendarEvent>()), Times.Never);
        _calendarRepositoryMock.VerifyNoOtherCalls();

        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(CalendarTestData.DefaultUpdateEvent.CategoryId), Times.Once);
        _categoryRepositoryMock.VerifyNoOtherCalls();
    }

    // DeleteEventAsync
    [Fact]
    public async Task DeleteEventAsync_WhenEventExists_CallsRepositoryDelete()
    {
        // Arrange
        _getByIdResponse = CalendarTestData.DefaultEvent;
        var service = GetService();

        // Act
        await service.DeleteEventAsync(ID);

        // Assert
        _calendarRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(ID), Times.Once);
        _calendarRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteEventAsync_WhenEventDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _getByIdResponse = null;
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.DeleteEventAsync(ID);

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _calendarRepositoryMock.Verify(r => r.GetByIdAsync(ID), Times.Once);
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
        _calendarRepositoryMock.VerifyNoOtherCalls();
    }

    // GetHolidaysAsync
    [Fact]
    public async Task GetHolidaysAsync_WhenHolidaysExist_ReturnsMappedHolidays()
    {
        // Arrange
        var holidays = new List<Holiday> { CalendarTestData.DefaultHoliday };
        _getHolidaysResponse = holidays;
        var service = GetService();

        // Act
        var result = await service.GetHolidaysAsync("2026");

        // Assert
        AssertEx.AreEqual(result, holidays.Adapt<List<HolidayDto>>());

        _apiClientMock.Verify(a => a.GetHolidaysAsync("2026"), Times.Once);
        _apiClientMock.VerifyNoOtherCalls();
        _calendarRepositoryMock.VerifyNoOtherCalls();
        _todoRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetHolidaysAsync_WhenApiReturnsNoHolidays_ThrowsNotFoundException()
    {
        // Arrange
        _getHolidaysResponse = new List<Holiday>();
        var service = GetService();

        // Act
        Func<Task> act = async () => await service.GetHolidaysAsync("2026");

        // Assert
        await AssertEx.Throws<NotFoundException>(act);

        _apiClientMock.Verify(a => a.GetHolidaysAsync("2026"), Times.Once);
        _apiClientMock.VerifyNoOtherCalls();
        _calendarRepositoryMock.VerifyNoOtherCalls();
        _todoRepositoryMock.VerifyNoOtherCalls();
    }

    // GetICalLinkAsync
    [Fact]
    public async Task GetICalLinkAsync_WhenEndDateTimeIsSet_ReturnsLinkWithAllQueryParameters()
    {
        // Arrange
        var calendarEvent = CalendarTestData.DefaultICalLinkEvent;
        var service = GetService();

        // Act
        var result = await service.GetICalLinkAsync(calendarEvent);

        // Assert
        var query = ParseQuery(result);
        AssertEx.AreEqual("Team Meeting", query["title"]);
        AssertEx.AreEqual("2026-06-01T09:00:00.0000000Z", query["start"]);
        AssertEx.AreEqual("2026-06-01T10:00:00.0000000Z", query["end"]);
        AssertEx.AreEqual("Besprechung Projekt Updates", query["description"]);

        _calendarRepositoryMock.VerifyNoOtherCalls();
        _todoRepositoryMock.VerifyNoOtherCalls();
        _apiClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetICalLinkAsync_WhenEndDateTimeIsNull_AddsOneHourToStartAsEnd()
    {
        // Arrange
        var calendarEvent = CalendarTestData.DefaultICalLinkEvent with { EndDateTime = null };
        var service = GetService();

        // Act
        var result = await service.GetICalLinkAsync(calendarEvent);

        // Assert
        var query = ParseQuery(result);
        AssertEx.AreEqual("2026-06-01T09:00:00.0000000Z", query["start"]);
        AssertEx.AreEqual("2026-06-01T10:00:00.0000000Z", query["end"]);

        _calendarRepositoryMock.VerifyNoOtherCalls();
        _todoRepositoryMock.VerifyNoOtherCalls();
        _apiClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetICalLinkAsync_WhenEndDateTimeEqualsStartDateTime_AddsOneHourToEnd()
    {
        // Arrange
        var calendarEvent = CalendarTestData.DefaultICalLinkEvent with
        {
            EndDateTime = CalendarTestData.DefaultICalLinkEvent.StartDateTime
        };
        var service = GetService();

        // Act
        var result = await service.GetICalLinkAsync(calendarEvent);

        // Assert
        var query = ParseQuery(result);
        AssertEx.AreEqual("2026-06-01T09:00:00.0000000Z", query["start"]);
        AssertEx.AreEqual("2026-06-01T10:00:00.0000000Z", query["end"]);

        _calendarRepositoryMock.VerifyNoOtherCalls();
        _todoRepositoryMock.VerifyNoOtherCalls();
        _apiClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetICalLinkAsync_WhenNoteIsNull_SetsEmptyDescription()
    {
        // Arrange
        var calendarEvent = CalendarTestData.DefaultICalLinkEvent with { Note = null };
        var service = GetService();

        // Act
        var result = await service.GetICalLinkAsync(calendarEvent);

        // Assert
        var query = ParseQuery(result);
        AssertEx.AreEqual(string.Empty, query["description"]);
        AssertEx.AreEqual("Team Meeting", query["title"]);

        _calendarRepositoryMock.VerifyNoOtherCalls();
        _todoRepositoryMock.VerifyNoOtherCalls();
        _apiClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetICalLinkAsync_WhenEndDateTimeIsBeforeStartDateTime_AddsOneHourToEnd()
    {
        // Arrange
        var calendarEvent = CalendarTestData.DefaultICalLinkEvent with
        {
            EndDateTime = CalendarTestData.DefaultICalLinkEvent.StartDateTime.AddMinutes(-30)
        };
        var service = GetService();

        // Act
        var result = await service.GetICalLinkAsync(calendarEvent);

        // Assert
        var query = ParseQuery(result);
        AssertEx.AreEqual("2026-06-01T09:00:00.0000000Z", query["start"]);
        AssertEx.AreEqual("2026-06-01T09:30:00.0000000Z", query["end"]);

        _calendarRepositoryMock.VerifyNoOtherCalls();
        _todoRepositoryMock.VerifyNoOtherCalls();
        _apiClientMock.VerifyNoOtherCalls();
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
        var service = GetService();

        // Act
        var result = await service.GetICalLinkAsync(calendarEvent);

        // Assert
        var query = ParseQuery(result);
        AssertEx.AreEqual(true, query["start"]!.EndsWith("Z"));
        AssertEx.AreEqual(true, query["end"]!.EndsWith("Z"));
        AssertEx.AreEqual(
            calendarEvent.StartDateTime.ToUniversalTime(),
            DateTime.Parse(query["start"]!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal));

        _calendarRepositoryMock.VerifyNoOtherCalls();
        _todoRepositoryMock.VerifyNoOtherCalls();
        _apiClientMock.VerifyNoOtherCalls();
    }

    // DeletePastEventsAsync
    [Fact]
    public async Task DeletePastEventsAsync_WhenNonRecurringEventIsOldEnough_DeletesEvent()
    {
        // Arrange
        var threshold = DateTime.Now.AddMonths(-3);
        var oldEvent = CalendarTestData.DefaultEvent;
        oldEvent.StartDateTime = threshold.AddDays(-10);
        oldEvent.EndDateTime = threshold.AddDays(-5);
        oldEvent.RecurrenceRule = null;

        _getEventsOlderThanResponse = new List<CalendarEvent> { oldEvent };
        var service = GetService();

        // Act
        await service.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()), Times.Once);
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(oldEvent.Id), Times.Once);
        _calendarRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenNonRecurringEventIsNotOldEnough_DoesNotDelete()
    {
        // Arrange
        var threshold = DateTime.Now.AddMonths(-3);
        var recentEvent = CalendarTestData.DefaultEvent;
        recentEvent.StartDateTime = threshold.AddDays(5);
        recentEvent.EndDateTime = threshold.AddDays(10);
        recentEvent.RecurrenceRule = null;

        _getEventsOlderThanResponse = new List<CalendarEvent> { recentEvent };
        var service = GetService();

        // Act
        await service.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()), Times.Once);
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
        _calendarRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenEventHasNoEndDate_UsesStartDateAsThreshold()
    {
        // Arrange
        var threshold = DateTime.Now.AddMonths(-3);
        var eventWithoutEnd = CalendarTestData.DefaultEvent;
        eventWithoutEnd.StartDateTime = threshold.AddDays(-1);
        eventWithoutEnd.EndDateTime = null;
        eventWithoutEnd.RecurrenceRule = null;

        _getEventsOlderThanResponse = new List<CalendarEvent> { eventWithoutEnd };
        var service = GetService();

        // Act
        await service.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()), Times.Once);
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(eventWithoutEnd.Id), Times.Once);
        _calendarRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenRecurringEventHasNoRecurrenceEnd_DoesNotDelete()
    {
        // Arrange
        var oldEvent = CalendarTestData.DefaultEvent;
        oldEvent.StartDateTime = DateTime.Now.AddYears(-2);
        oldEvent.RecurrenceRule = "FREQ=WEEKLY";
        oldEvent.RecurrenceEnd = null;

        _getEventsOlderThanResponse = new List<CalendarEvent> { oldEvent };
        var service = GetService();

        // Act
        await service.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()), Times.Once);
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
        _calendarRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenRecurrenceEndIsOldEnough_DeletesRecurringEvent()
    {
        // Arrange
        var threshold = DateTime.Now.AddMonths(-3);
        var oldRecurring = CalendarTestData.DefaultEvent;
        oldRecurring.StartDateTime = threshold.AddYears(-1);
        oldRecurring.RecurrenceRule = "FREQ=WEEKLY";
        oldRecurring.RecurrenceEnd = threshold.AddDays(-1);

        _getEventsOlderThanResponse = new List<CalendarEvent> { oldRecurring };
        var service = GetService();

        // Act
        await service.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()), Times.Once);
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(oldRecurring.Id), Times.Once);
        _calendarRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenRecurrenceEndIsNotOldEnough_DoesNotDelete()
    {
        // Arrange
        var threshold = DateTime.Now.AddMonths(-3);
        var activeRecurring = CalendarTestData.DefaultEvent;
        activeRecurring.StartDateTime = threshold.AddYears(-1);
        activeRecurring.RecurrenceRule = "FREQ=WEEKLY";
        activeRecurring.RecurrenceEnd = threshold.AddDays(10);

        _getEventsOlderThanResponse = new List<CalendarEvent> { activeRecurring };
        var service = GetService();

        // Act
        await service.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()), Times.Once);
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
        _calendarRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeletePastEventsAsync_WhenNoEventsExist_DoesNotCallDelete()
    {
        // Arrange
        _getEventsOlderThanResponse = new List<CalendarEvent>();
        var service = GetService();

        // Act
        await service.DeletePastEventsAsync(3);

        // Assert
        _calendarRepositoryMock.Verify(r => r.GetEventsOlderThanAsync(It.IsAny<DateTime>()), Times.Once);
        _calendarRepositoryMock.Verify(r => r.DeleteEventAsync(It.IsAny<int>()), Times.Never);
        _calendarRepositoryMock.VerifyNoOtherCalls();
    }

    private static NameValueCollection ParseQuery(string link)
    {
        var query = link[(link.IndexOf('?') + 1)..];
        return HttpUtility.ParseQueryString(query);
    }
}
