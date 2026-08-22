---
applyTo:
  - "**/*Tests.cs"
  - "**/*.Tests/**/*.cs"
  - "**/*Test.cs"
---

# Test Conventions

## Purpose

Conventions for all unit tests in the .NET backend (controller tests, application-/service tests,
repository tests). Complements `02-backend.instructions.md`; the Microsoft .NET/C# conventions
are the baseline (see `01-general.instructions.md`).

## Naming Convention

Test methods are always named according to the following scheme:

```
MethodUnderTest_Condition_ExpectedBehavior
```

Example:

```csharp
GetEventByIdAsync_WhenEventExists_ReturnsEvent
```

Otherwise, the common C# naming conventions for test classes and methods apply.

## Triple-A (Arrange, Act, Assert)

Every test must be clearly divided into the three sections and annotated with comments:

```csharp
[Fact]
public async Task GetEventByIdAsync_WhenEventExists_ReturnsEvent()
{
    // Arrange
    ...

    // Act
    ...

    // Assert
    ...
}
```

A test without these three comments (even if a section is trivial) is considered incomplete.

## Mocking, Verifications & Detailed Assertions

### Mandatory `.Verify(...)` for Mocked Calls
In all unit tests involving mocked dependencies (e.g., Controller tests and Service tests using Moq):
- **Every test must explicitly verify expected mock methods were invoked** in the `// Assert` block using `.Verify(...)` with `Times.Once` (or exact expected call count).
- **Negative / exception paths** must explicitly verify that mutation or side-effect methods were **NEVER** called using `Times.Never()`.

### Strict Argument Matching – Usage of `It.IsAny<T>()`
- **In `.Setup(...)`**: `It.IsAny<T>()` is completely valid and expected when configuring mock responses (e.g., in `GetController()` default setups `_serviceMock.Setup(s => s.CreateTodoAsync(It.IsAny<CreateTodoDto>())).ReturnsAsync(_createTodoResponse)`).
- **In `.Verify(...)`**: Avoid `It.IsAny<T>()` when verifying expected invocations on positive/happy paths. Always check actual arguments passed using exact values or property predicate matchers (`It.Is<T>(x => ...)`).
- **Negative paths in `.Verify(...)`**: Using `It.IsAny<T>()` in `.Verify(...)` is appropriate when verifying that a method was **NEVER** called on negative paths (e.g., `_serviceMock.Verify(s => s.UpdateTodoAsync(It.IsAny<int>(), It.IsAny<UpdateTodoDto>()), Times.Never)`).

#### Examples

```csharp
[Fact]
public async Task UpdateTodoAsync_WhenTodoExists_UpdatesPropertiesAndCallsRepository()
{
    // Arrange
    var existingTodo = TodoTestData.DefaultTodo;
    var updateDto = new UpdateTodoDto("New Title", "New Description", 2, 1, DateTime.Today);

    todoRepositoryMock
        .Setup(r => r.GetTodoByIdAsync(existingTodo.Id))
        .ReturnsAsync(existingTodo);

    // Act
    await service.UpdateTodoAsync(existingTodo.Id, updateDto);

    // Assert
    // Verify method call with exact parameter matchers (No It.IsAny)
    todoRepositoryMock.Verify(r => r.UpdateTodoAsync(It.Is<Todo>(t =>
        t.Id == existingTodo.Id &&
        t.Title == updateDto.Title &&
        t.Description == updateDto.Description &&
        t.CategoryId == updateDto.CategoryId &&
        t.Priority == updateDto.Priority &&
        t.DueDate == updateDto.DueDate
    )), Times.Once);
}
```

```csharp
[Fact]
public async Task UpdateTodoAsync_WhenTodoDoesNotExist_ThrowsNotFoundExceptionAndDoesNotCallUpdate()
{
    // Arrange
    var updateDto = new UpdateTodoDto("New Title", "New Description", 2, 1, DateTime.Today);

    todoRepositoryMock
        .Setup(r => r.GetTodoByIdAsync(999))
        .ReturnsAsync((Todo?)null);

    // Act & Assert
    await FluentActions.Invoking(() => service.UpdateTodoAsync(999, updateDto))
        .Should().ThrowAsync<NotFoundException>();

    // Explicitly verify UpdateTodoAsync was NEVER called
    todoRepositoryMock.Verify(r => r.UpdateTodoAsync(It.IsAny<Todo>()), Times.Never);
}
```

### Comprehensive Assertions
Unit tests must feature deep and thorough assertions in the `// Assert` block:
- **Return Values**: Assert returned objects completely. Check properties, mapped fields, status flags, and timestamps.
- **Collections**: Assert count, exact ordering, and item properties (do not stop at simple `.Should().NotBeNull()`).
- **Side Effects & State Changes**: Assert mutated properties on entities and verify state transitions explicitly.
- **Exception Details**: For negative tests, assert the exception type as well as the exception message content.

## Standard Controller Test Class Structure

All controller test classes (`*ControllerTests.cs`) **must follow a mandatory, standardized layout structure**:

1. **Private Readonly Mock Fields**: Declare mock fields for injected services (e.g., `private readonly Mock<ITodoService> _todoServiceMock;`).
2. **Private Default Response Fields**: Declare private fields for default return values (e.g., `private List<TodoDto> _getTodosResponse;`, `private int _createTodoResponse;`).
3. **Static Test Data Constants**: Declare `private static readonly int ID = TestData.DefaultItem.Id;` using shared test data from `Multitool.Tests.Shared`.
4. **Constructor**:
   - Instantiate mock objects (`_todoServiceMock = new Mock<ITodoService>();`).
   - Initialize default response fields with shared test data.
5. **Private `GetController()` Factory Method**:
   - Resets mock states via `_todoServiceMock.Reset()`.
   - Configures default setups (`_todoServiceMock.Setup(...).ReturnsAsync(...)` or `.Returns(Task.CompletedTask)`).
   - Instantiates and returns the target controller (`return new TodoController(_todoServiceMock.Object);`).
6. **Route Separator Comments**: Visually group test methods using `// [HTTP Verb] api/[Controller]/[Route]`.
7. **Triple-A & AssertEx Assertions**:
   - Divide tests cleanly into `// Arrange`, `// Act`, and `// Assert`.
   - Use `AssertEx` helpers for HTTP responses (`AssertEx.Ok(result, expected)`, `AssertEx.Created(result, id)`, `AssertEx.NoContent(result)`, `AssertEx.BadRequest(result)`, `AssertEx.NotFound(result)`).
8. **Strict Moq Verification & No Other Calls**:
   - Verify expected service calls with exact arguments (`_todoServiceMock.Verify(s => s.CreateTodoAsync(newTodo), Times.Once);`).
   - Always call `_todoServiceMock.VerifyNoOtherCalls();` to ensure no unexpected service interactions occurred.

### Example (Controller Test Layout)

```csharp
public class TodoControllerTests
{
    private readonly Mock<ITodoService> _todoServiceMock;

    private List<TodoDto> _getTodosResponse;
    private int _createTodoResponse;

    private static readonly int ID = TodoTestData.DefaultTodo.Id;

    public TodoControllerTests()
    {
        _todoServiceMock = new Mock<ITodoService>();

        // Default responses
        _getTodosResponse = new List<TodoDto>() { TodoTestData.DefaultTodoDto };
        _createTodoResponse = TodoTestData.DefaultTodo.Id;
    }

    private TodoController GetController()
    {
        _todoServiceMock.Reset();

        _todoServiceMock.Setup(s => s.GetTodosAsync())
            .ReturnsAsync(_getTodosResponse);

        _todoServiceMock.Setup(s => s.CreateTodoAsync(It.IsAny<CreateTodoDto>()))
            .ReturnsAsync(_createTodoResponse);

        _todoServiceMock.Setup(s => s.UpdateTodoAsync(It.IsAny<int>(), It.IsAny<UpdateTodoDto>()))
            .Returns(Task.CompletedTask);

        _todoServiceMock.Setup(s => s.DeleteTodoAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        return new TodoController(_todoServiceMock.Object);
    }

    // GET api/Todo

    [Fact]
    public async Task GetTodos_WhenTodosExist_ReturnsOkWithTodos()
    {
        // Arrange
        var controller = GetController();

        // Act
        var result = await controller.GetTodos();

        // Assert
        AssertEx.Ok(result, _getTodosResponse);

        _todoServiceMock.Verify(s => s.GetTodosAsync(), Times.Once);
        _todoServiceMock.VerifyNoOtherCalls();
    }

    // POST api/Todo

    [Fact]
    public async Task CreateTodo_WhenDtoIsValid_ReturnsCreatedWithId()
    {
        // Arrange
        var newTodo = TodoTestData.DefaultCreateTodoDto;
        var controller = GetController();

        // Act
        var result = await controller.CreateTodo(newTodo);

        // Assert
        AssertEx.Created(result, _createTodoResponse);

        _todoServiceMock.Verify(s => s.CreateTodoAsync(newTodo), Times.Once);
        _todoServiceMock.VerifyNoOtherCalls();
    }
}
```

## Separator comments between tested methods

To visually group tests by the tested method/route, a separator comment is placed before the first test method
for a specific controller action or service/repository method.
All tests that test the same method are placed directly below one another under this single comment
(the comment is not repeated per test).

### Controller tests

Format: `// [HTTP Verb] api/[Controller]/[Route]` – exactly as the route is called.

```csharp
// POST api/Auth/register
[Fact]
public async Task Register_WhenEmailIsValid_ReturnsOk()
{
    // Arrange
    ...

    // Act
    ...

    // Assert
    ...
}

[Fact]
public async Task Register_WhenEmailAlreadyExists_ReturnsConflict()
{
    // Arrange
    ...

    // Act
    ...

    // Assert
    ...
}

// GET api/Auth/me
[Fact]
public async Task Me_WhenUserIsAuthenticated_ReturnsUser()
{
    // Arrange
    ...

    // Act
    ...

    // Assert
    ...
}
```

### Application-/service and repository tests

Here, the plain method name as a separator comment is sufficient:

```csharp
// GetEventByIdAsync
[Fact]
public async Task GetEventByIdAsync_WhenEventExists_ReturnsEvent()
{
    // Arrange
    ...

    // Act
    ...

    // Assert
    ...
}

[Fact]
public async Task GetEventByIdAsync_WhenEventDoesNotExist_ReturnsNull()
{
    // Arrange
    ...

    // Act
    ...

    // Assert
    ...
}
```

### Shared test data (`tests/Multitool.Tests.Shared`)

To ensure consistent and reusable test data across all test projects (`Multitool.Api.Tests`, `Multitool.Application.Tests`, `Multitool.Infrastructure.Tests`), all general test data is maintained centrally in the shared class library project:

```
tests/Multitool.Tests.Shared
```

#### Mandatory Rule: Synchronizing TestData with Models
- **Whenever a test-relevant entity, DTO, or model is created or updated** in the application or domain layers (`src/Multitool.Domain` or `src/Multitool.Application`), corresponding default test data **must immediately be added or updated in `tests/Multitool.Tests.Shared`** (e.g., `TodoTestData.cs`, `CalendarTestData.cs`).
- Each module's TestData class must provide static getter properties (`=>`) for:
  - Default Domain Entities (`DefaultTodo => new() { ... }`)
  - Default Response DTOs (`DefaultTodoDto => ...`)
  - Default Request DTOs (`DefaultCreateTodoDto => ...`, `DefaultUpdateTodoDto => ...`)
- **Do NOT construct ad-hoc inline DTOs or entities** inside individual test files if a shared counterpart exists or should exist in `Multitool.Tests.Shared`.

#### Modifying test data in tests (Arrange)

If a test intentionally wants to test a modified value or negative scenario, it takes the shared default from `Multitool.Tests.Shared` and mutates or overrides fields locally in the `// Arrange` block:

```csharp
// Arrange
var calendarEvent = CalendarTestData.DefaultEvent;
calendarEvent.CategoryId = 2;
```

Important:

- Even modified test data must always be based on the shared defaults.
- Tests should not create deviating "mini worlds".
- Modifications are made exclusively in the respective test, never globally.

## What tests are expected per layer?

### Controller

Controller tests **exclusively verify the HTTP flow** (HTTP status code, response payload, and service delegation). Business logic and domain validations are **never** tested in controller tests.

- **No response variation**: Controller tests do not vary default mock responses (e.g., testing empty lists vs populated lists or edge-case data). All data variations and business logic edge cases belong exclusively in the service layer tests.
- **Exactly one test per endpoint route** is expected (unless the controller itself builds an anonymous object or transforms parameters).

| Return | Expected test |
|---|---|
| `return Ok(result)` | `WhenXExists_ReturnsOkWith...` |
| `return NoContent()` | `WhenXExists_ReturnsNoContent` |
| `return StatusCode(StatusCodes.Status201Created, id)` | `WhenDtoIsValid_ReturnsCreatedWithId` |

**Exceptions – a second test is justified when:**
- The controller itself builds an anonymous object (`new { year, month, count }`)
- The controller transforms parameters (`categories ?? string.Empty`)

**Do not test in controller tests:**
- Business logic, domain rules, or service exception propagation (`ThrowsException`, `ThrowsNotFoundException`) – no try/catch in the controller → exception handling is tested in GlobalExceptionHandler and Service tests.
- Trivial parameter forwarding (`ForwardsXToService`) – only tests C# language behavior.
- Data variations / edge cases (empty list vs populated list) – tested exclusively in the service layer.

---

### Service

The service contains the **business logic** – this is where most tests are expected.

#### Happy path (always)
- Method returns the correct result
- Correct fields are set (mapping, calculations)
- Repository is called with the right parameters

#### Negative paths (if present)
- `WhenXDoesNotExist_ThrowsNotFoundException` – when `Get<Module>ByIdAsync` returns null
- `WhenXIsLocked_ThrowsInvalidOperationException` – for status checks
- `WhenAdminKeyIsInvalid_ThrowsInvalidCredentialException` – for validations

#### Logic branches (test all branches)
- `bool` flags: both directions (`typeChanged: true` and `typeChanged: false`)
- Toggle logic: `WhenIsDoneIsFalse_SetsToTrue` + `WhenIsDoneIsTrue_SetsToFalse`
- `GetOrCreate` pattern: `WhenSettingsExist_Returns...` + `WhenSettingsDoNotExist_CreatesDefault`

#### Mapping (if Adapt/Mapster is used)
- One test that verifies the relevant fields are mapped correctly
- Include server-managed fields (`CreatedAt`, `IsDone`, etc.) in the assertions
- No test for every single field – only the non-trivial ones
- `Create<Entity>Dto` has no `Id` – assert that server-generated ids/timestamps are set by the service, not passed in
- Updates assign fields explicitly (`existing.X = dto.X`) – the update test asserts the mutated fields on the existing object

#### Time-dependent values
- `CreationDateTime`, `LockoutEnd`, `ExpiresAt` etc. with `BeCloseTo(..., TimeSpan.FromSeconds(5))`

---

### Repository (integration tests against DB)

The repository is tested **against a real (in-memory/SQLite) database** – no mocks.

#### Always test
- **Add/Insert:** the record exists in the DB afterwards
- **GetById / GetBy...:** returns the correct record / returns null if not present
- **Update:** fields are updated in the DB afterwards
- **Delete:** the record no longer exists afterwards

#### Query logic (for complex Where conditions)
- Test each condition individually (overlap, exclusion, boundary value)
- Boundary values explicitly: `WhenEventIsOnEndDate_IsExcluded` (exclusive vs. inclusive range)
- Sorting: `WhenMultipleExist_ReturnsSortedBy...`
- Filter: `WhenCategoryFilterApplied_ReturnsOnlyMatchingEntries`

#### ExecuteDeleteAsync / ExecuteUpdateAsync
- Use `Context.ChangeTracker.Clear()` or `AsNoTracking().FirstOrDefaultAsync()` after the delete
- Otherwise `FindAsync` returns the cached tracker entry → incorrect result

#### Do not test (repository)
- Simple `FindAsync` wrappers without logic of their own – no dedicated test needed
- `EF.Functions.ILike` / Postgres-specific functions with SQLite – separate test with Testcontainers, or rewrite the method to use `ToLower().Contains()`

---

### GlobalExceptionHandler
- Test exception mappings with `[Theory]` + `[InlineData]` (one test for all exception types)
- Verify: the correct HTTP status code is set
- Do not test in the controller – exclusively in the handler itself

---

## General Rules

### What is always tested
- Every `if` branch that contains logic of its own
- Every `throw` path
- Every calculation with concrete, traceable values
- Every explicit mapping (non-trivial forwarding)

### What is never tested
- C# language behavior (parameters are forwarded, exceptions propagate)
- Trivial forwarding without logic of its own
- Impossible scenarios (that can never occur due to service logic)
- Exception propagation in the controller (GlobalExceptionHandler is responsible)

### Theory instead of multiple Facts
Use `[Theory]` + `[InlineData]` when:
- Same test logic, different input values (e.g., multiple `DayStatus` values)
- Multiple data types that go through the same code path (e.g., `CustomDataType.Int/Decimal/Bool`)

```csharp
[Theory]
[InlineData(DayStatus.Vacation)]
[InlineData(DayStatus.Sick)]
public async Task CreateWorkDayAsync_WhenStatusIsVacationOrSick_SetsWorkToZero(DayStatus status)
```

No `[Theory]` if the assertions differ per case – then use separate `[Fact]` tests.

### TestData classes
- Use **Properties instead of Fields** to avoid mutation between tests:

```csharp
// ❌ Wrong – shared mutable state
public static readonly Todo DefaultTodo = new() { ... };

// ✅ Correct – new instance per access
public static Todo DefaultTodo => new() { ... };
```

- Test objects that are only needed in a single test class: `private static readonly` directly in the test class

### Expected number of tests per method

| Layer | Simple method | Method with branches | Method with calculation |
|---|---|---|---|
| Controller | 1 | 1–2 | 1 |
| Service | 1–2 | 2–4 | 2–3 |
| Repository | 1–2 | 2–4 | – |

---

## Review Checklist for Unit Tests

When reviewing, please check:

- Does the test name exactly follow the scheme `Method_Condition_ExpectedBehavior`?
- Are `// Arrange`, `// Act`, `// Assert` present and correctly assigned?
- Is the appropriate separator comment placed before the first test method for a new tested method/route
  (`// [Verb] api/[Controller]/[Route]` for controller tests,
  `// MethodName` for application-/repository tests)?
- Do tests for the same tested method appear directly below one another, without repeating the comment?
- Was the separator comment not unnecessarily re-added for a new test of an existing method?
- Is at most one test expected per controller route (except for `CreatedAtAction`, anonymous objects, or parameter transformation)?
- Are no exception tests written in the controller (responsibility lies with the GlobalExceptionHandler)?
- Are no trivial forwarding tests written (`ForwardsXToService`)?
- Are impossible scenarios not tested (e.g., empty list when the service throws an exception)?
- Are all `if` branches and `throw` paths in the service tested?
- Is `[Theory]` + `[InlineData]` used when the same logic is tested with different input values?
- Do repository tests use `AsNoTracking().FirstOrDefaultAsync()` instead of `FindAsync` after `ExecuteDeleteAsync`?
- Are TestData properties (`=>`) used instead of fields (`= new()`) to avoid mutation between tests?
- Are test-relevant entities, DTOs, and models added or updated in `tests/Multitool.Tests.Shared` whenever application models change (instead of creating ad-hoc inline test objects)?
- Do controller test classes follow the standard controller test layout (`GetController()` factory method, mock resets, default response fields, `AssertEx` assertions, and `.VerifyNoOtherCalls()`)?
- Are all expected mock method calls verified in the `// Assert` block using `.Verify(...)` (with `Times.Once` for expected calls and `Times.Never` for negative paths)?
- Is `It.IsAny<T>()` avoided in favor of exact values or `It.Is<T>(...)` parameter matchers?
- Are return values, collection contents, mapped properties, and side effects thoroughly asserted instead of relying on simple null/not-null checks?
