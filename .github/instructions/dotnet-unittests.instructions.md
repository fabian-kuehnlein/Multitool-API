---
applyTo:
  - "**/*Tests.cs"
  - "**/*.Tests/**/*.cs"
  - "**/*Test.cs"
---

# .NET Unit Test Guidelines

## Purpose

Conventions for unit tests in the .NET backend (controller tests, application-/service tests,
repository tests). Complements `dotnet-backend.instructions.md`.

## Naming Convention

Test methods are always named according to the following scheme:

```
MethodUnderTest_Condition_ExpectedBehavior
```

Example:

```csharp
GetByIdAsync_WhenEventExists_ReturnsEvent
```

Otherwise, the common C# naming conventions for test classes and methods apply.

## Triple-A (Arrange, Act, Assert)

Every test must be clearly divided into the three sections and annotated with comments:

```csharp
[Fact]
public async Task GetByIdAsync_WhenEventExists_ReturnsEvent()
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
// GetByIdAsync
[Fact]
public async Task GetByIdAsync_WhenEventExists_ReturnsEvent()
{
    // Arrange
    ...

    // Act
    ...

    // Assert
    ...
}

[Fact]
public async Task GetByIdAsync_WhenEventDoesNotExist_ReturnsNull()
{
    // Arrange
    ...

    // Act
    ...

    // Assert
    ...
}
```

### Shared test data

To ensure consistent and reusable test data across all test projects, all general test data is provided centrally
in the class library tests/Multitool.Tests.Shared.

This library contains exclusively shared test data, e.g.:

- CalendarTestData.DefaultEvent
- CustomTableTestData.DefaultTable
- UserTestData.DefaultUser

#### Modifying test data (Arrange)

If a test intentionally wants to produce a deviating or erroneous result, this is done in the Arrange block
of the respective test method. Example:

```csharp
// Arrange
var event = CalendarEvent CalendarTestData.DefaultEvent;
event.CategoryId = 2;
```

Important:

- Even modified test data must always be based on the shared defaults.
- Tests should not create deviating "mini worlds".
- Modifications are made exclusively in the respective test, never globally.

## What tests are expected per layer?

### Controller

The controller has **no business logic of its own** – it only delegates to the service. Accordingly, **exactly one test** is expected per route.

| Return | Expected test |
|---|---|
| `return Ok(result)` | `WhenXExists_ReturnsOkWith...` |
| `return NoContent()` | `WhenXExists_ReturnsNoContent` |
| `return CreatedAtAction(...)` | `WhenDtoIsValid_ReturnsCreatedAtAction` + `WhenDtoIsValid_SetsCorrectRouteValues` |

**Exceptions – a second test is justified when:**
- The controller itself builds an anonymous object (`new { year, month, count }`)
- The controller transforms parameters (`categories ?? string.Empty`)
- `CreatedAtAction` is used – here a separate test for `ActionName` and `RouteValues` is worthwhile

**Do not test:**
- Exception propagation (`ThrowsException`, `ThrowsNotFoundException`) – no try/catch in the controller → no test
- Trivial parameter forwarding (`ForwardsXToService`) – only tests C# language behavior
- Impossible scenarios (e.g., empty list when the service throws an exception on an empty result)

---

### Service

The service contains the **business logic** – this is where most tests are expected.

#### Happy path (always)
- Method returns the correct result
- Correct fields are set (mapping, calculations)
- Repository is called with the right parameters

#### Negative paths (if present)
- `WhenXDoesNotExist_ThrowsNotFoundException` – when `GetByIdAsync` returns null
- `WhenXIsLocked_ThrowsInvalidOperationException` – for status checks
- `WhenAdminKeyIsInvalid_ThrowsInvalidCredentialException` – for validations

#### Logic branches (test all branches)
- `bool` flags: both directions (`typeChanged: true` and `typeChanged: false`)
- Toggle logic: `WhenIsDoneIsFalse_SetsToTrue` + `WhenIsDoneIsTrue_SetsToFalse`
- `GetOrCreate` pattern: `WhenSettingsExist_Returns...` + `WhenSettingsDoNotExist_CreatesDefault`

#### Mapping (if Adapt/Mapster is used)
- One test that verifies the relevant fields are mapped correctly
- No test for every single field – only the non-trivial ones

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
