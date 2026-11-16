---
applyTo: "**/*.cs"
---

# Backend Conventions

## Purpose

This file defines the project-specific conventions for the .NET/C# production code
(controllers, services, repositories, models). It applies to `**/*.cs`.

Baseline are the Microsoft .NET/C# conventions (see `01-general.instructions.md`);
this file only adds what differs. For unit-test-specific rules, see `03-testing.instructions.md`.

## General

- Follow the common .NET/C# conventions (naming, async/await, dependency injection)
  unless something else is specified here.
- **No `try/catch` blocks in controllers.** Exceptions are caught and handled exclusively in the service layer.
  Controllers should stay "clean" and delegate the responsibility for
  error handling to the services (e.g., via global exception-handling middleware).
- Async methods should be consistently named with the `Async` suffix and return `Task`/`Task<T>`.

## Naming Pattern in Services and Repositories

CRUD and domain methods in **both Services and Repositories** must follow a strict, consistent naming pattern:

```
[Action][Module][Async]
```

- Always include the entity/module name in the method name (e.g., `CreateTodoAsync`, `GetTodoByIdAsync`, `UpdateWorkDayAsync`).
- If synchronous, omit `Async`.

### Examples
- **Create**: `CreateTodoAsync(Todo todo)` / `CreateTodoAsync(CreateTodoRequest dto)`
- **Read**: `GetTodoByIdAsync(int id)`, `GetTodosAsync()`, `GetWorkDaysAsync(DateTime start, DateTime end)`
- **Update**: `UpdateTodoAsync(Todo todo)`, `UpdateWorkDayAsync(WorkDay workDay)`
- **Delete**: `DeleteTodoAsync(Todo todo)`

Do **not** use generic method names like `AddAsync`, `GetByIdAsync`, `UpdateAsync`, or `DeleteAsync` in repositories or services — always specify the module/entity name.

## Exceptions only in the service layer & Service-Repository CRUD Flow

Exceptions should be thrown **exclusively in the service layer**.
Repositories deliver raw data and throw **no business exceptions** such as `NotFoundException`,
`ConflictException`, or validation errors.

### Standard Service-Repository Interaction Flow
For operations that modify or delete data, the process is strictly:
1. **Fetch Entity**: Service calls `repository.Get<Module>ByIdAsync(id)` to retrieve the entity.
2. **Check Existence & Validate**: Service checks if the entity exists (`null` check). If not, throws `NotFoundException` (or other appropriate business exception).
3. **Modify Entity**: Service updates the entity properties explicitly (or leaves the entity instance intact for deletion).
4. **Pass to Repository**: Service passes the full entity instance to `repository.Update<Module>Async(entity)` or `repository.Delete<Module>Async(entity)`.

**Important**: Repository delete and update methods must accept the full entity object (e.g., `DeleteTodoAsync(Todo todo)`), **never** just an `id` (e.g., `DeleteAsync(int id)` is prohibited).

### Examples

```csharp
public async Task DeleteTodoAsync(int id)
{
    var existingTodo = await todoRepository.GetTodoByIdAsync(id)

    if (existingTodo == null)
      throw new NotFoundException($"Todo with ID {id} not found.");

    await todoRepository.DeleteTodoAsync(existingTodo);
}
```

`````csharp
public async Task UpdateTodoAsync(int id, UpdateTodoRequest request)
{
    var existingTodo = await todoRepository.GetTodoByIdAsync(id)

    if (existingTodo == null)
        throw new NotFoundException($"Todo with ID {id} not found.");

    existingTodo.Title = request.Title;
    existingTodo.Description = request.Description;
    existingTodo.CategoryId = request.CategoryId;
    existingTodo.Priority = request.Priority;
    existingTodo.DueDate = request.DueDate;

    await todoRepository.UpdateTodoAsync(existingTodo);
}
```

## Application Models and Mapping (Mapster)

- **Domain entities have no suffix.**
- **Application input models use the `Request` suffix.**
- **Application output models use the `Model` suffix.**
- **Do not introduce new classes with the `Dto` suffix.**

All object transformation between application models and entities is done with **Mapster**. There is exactly one convention
for how objects flow through the services â€” no mix of manual object creation and Mapster mapping.

### Record-based application models (immutable)

Application input/output models used for data transfer (requests and responses) **must be positional records without `{ get; set; }` properties**:

```csharp
// âœ… Correct - Immutable positional request record
public record CreateTodoRequest(
    string Title,
    string? Description,
    int CategoryId,
    int Priority,
    DateTime? DueDate);

// âŒ Incorrect - Record or class with { get; set; } property bodies
public record TodoModel
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
}
```

- Data is received as immutable request models from controllers and passed into services.
- In the service layer, request models are mapped to domain entities (or entities are mapped to response models where necessary).

### Action-specific request/response models

Every request and every response carries a dedicated application model. The naming scheme is:

| Model | Purpose | Contains |
|---|---|---|
| `Create<Entity>Request` | Request body of a `POST` action | Only client-supplied fields. **Never an `Id`**, never server-managed fields. |
| `Update<Entity>Request` | Request body of a `PUT`/`PATCH` action | Only mutable fields. **Never an `Id`** â€” the id comes from the route. |
| `<Entity>Model` / purpose-specific response model | Response | The full serializable view, including the `Id`. |

- **`Create<Entity>Request` has no `Id`.** The `Id` is generated by the database and set on the entity, never accepted
  from the client. The same applies to all server-managed fields (timestamps, default flags).
- **`Update<Entity>Request` never contains an `Id` either.** The `Id` comes from the route
  (`PUT api/[controller]/[resource]/{id}`), consistently across all update routes. The route identifies the existing
  object; the body carries only the mutable fields.
- **Domain entities are never bound via `[FromBody]` and never serialized as a response.** The controller receives
  request models and returns response models (or primitive/status results).

### Mapping via Mapster

- **Creations and reads** run through Mapster. Custom mappings are registered **once** in
  `MappingConfig.cs` via `NewConfig<TSource, TDest>()` + `.Map(...)`; services only call `.Adapt<T>()`.
- Default / server-managed values are mapped in `MappingConfig.cs`, not assigned by hand in the service:

```csharp
// MappingConfig.cs
config.NewConfig<CreateTodoRequest, Todo>()
    .Map(dest => dest.IsDone, src => false)
    .Map(dest => dest.CreationDateTime, src => DateTime.Now);
```

- **Updates of existing database objects use explicit property-by-property assignment**
  (`existing.Name = request.Name;`) instead of `request.Adapt(existing)`. The changes must be visible at a glance;
  `Adapt` on an existing entity hides which fields are being modified.
- Manual object creation (`new Entity { ... }`) in services is **not allowed for pure copying**.
  It is only acceptable where real business logic is involved (calculations, aggregations, combining multiple sources).

### Example

```csharp
public async Task<int> CreateTodoAsync(CreateTodoRequest request)
{
    var todo = request.Adapt<Todo>();
    
    return await todoRepository.CreateTodoAsync(todo);
}
```

Update â€” explicit assignment so every change is visible:

```csharp
public async Task UpdateTodoAsync(int id, UpdateTodoRequest request)
{
    var existingTodo = await todoRepository.GetTodoByIdAsync(id)

    if (existingTodo == null)
        throw new NotFoundException($"Todo with ID {id} not found");

    existingTodo.Title = request.Title;
    existingTodo.Description = request.Description;
    existingTodo.DueDate = request.DueDate;

    await todoRepository.UpdateTodoAsync(existingTodo);
}
```

When reviewing, please check:

- Do CRUD method names in services and repositories strictly follow `[Action][Module][Async]` (e.g. `CreateTodoAsync`, `GetTodoByIdAsync`, `UpdateTodoAsync`, `DeleteTodoAsync`)?
- Does repository update/delete accept the full entity object instead of raw primitive IDs (after service fetches and validates existence)?
- Are application input/output models named with `Request`/`Model` suffixes (no `Dto` suffix)?
- Are request/response models positional records without `{ get; set; }` property bodies?
- Does a `Create<Entity>Request` or `Update<Entity>Request` contain an `Id` or another server-managed field? â†’ Violation.
- Is a domain entity bound via `[FromBody]` or returned directly as a response? â†’ Violation.
- Is an existing entity updated via `request.Adapt(existing)` instead of explicit property assignment? â†’ Violation.
- Are default / server-managed values mapped in `MappingConfig.cs` instead of in the service?

## Logging

Logging is used to find problems — not to flood the logs. Log only at structurally relevant points and avoid
trivial or high-frequency messages.

### Where to log (by layer)

- **Domain**: never log. The Domain layer contains no logging at all.
- **Application**: only log important business events (e.g. significant state changes worth tracing).
- **Infrastructure**: log errors, timeouts, and unusual situations (e.g. failing or timed-out external API calls).
- **Api**: log the start, end, and errors of each technical process (e.g. background cron jobs). Unhandled
  exceptions are logged by `GlobalExceptionHandler`.

### Rules

- Use **structured log messages with parameters** (`{Year}`, `{DeletedCount}`, `{Elapsed}`), never interpolated strings
  (`$"deleted {count} events"`).
- **Do not log** trivial method calls, loops, or parameter values that offer no diagnostic value (e.g. routine
  repository CRUD calls that always succeed).
- Errors must be logged together with the exception and the message should describe what was being attempted,
  e.g. `logger.LogError(ex, "Failed to fetch holidays for year {Year}", year)`.
- Log the outcome of an operation with meaningful summary data (counts, elapsed time), not per-item noise.

When reviewing, please check:

- Is any logging present in the Domain layer, or in trivial repository/application model code? → Violation.
- Are log messages using string interpolation (`$"..."`) instead of structured parameters? → Violation.
- Is a cron job missing start/end/error logging, or the external API client missing error/timeout logging? → Violation.

## Controllers

Every action method in a controller should be "decorated" as follows:

1. **XML documentation comment** (`/// <summary>`) with a short, understandable description of what the endpoint does.
2. Relevant attributes such as `[Authorize]` (if applicable).
3. HTTP method attribute with route, e.g., `[HttpGet("workdays")]`.
4. `[Produces("application/json")]`.
5. `[ProducesResponseType(...)]` for every realistically possible status code (at least the success case
   and `500 Internal Server Error`; where applicable also `400`, `401`, `404` etc., if these can actually occur).
6. The method itself stays slim: call the service, return the result. No `try/catch`.

### Return values for Creation (POST) actions

For POST / creation action methods in controllers, **do NOT use `Created()`, `CreatedAtAction()`, or `CreatedAtRoute()`** (as these require passing action string context).
Instead, explicitly return `StatusCode(StatusCodes.Status201Created, id)`:

```csharp
/// <summary>
/// Creates a new todo.
/// </summary>
[Authorize]
[HttpPost]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> CreateTodo([FromBody] CreateTodoRequest request)
{
    var id = await todoService.CreateTodoAsync(request);
    return StatusCode(StatusCodes.Status201Created, id);
}
```

### Return values for File Downloads

Endpoints that serve a downloadable file (e.g. an `.ics` calendar export) return a `File(...)` result instead of JSON.
The service returns the raw file bytes; the controller supplies the content type and a sanitized file name.

- Use `[Produces("<media type>")]` (e.g. `text/calendar`) instead of `application/json`.
- The service generates the file bytes (no URL/link string); do **not** delegate to an external file-generating service.

```csharp
/// <summary>
/// Generates an iCal (.ics) file for the given calendar event and returns it as a downloadable file.
/// </summary>
[Authorize]
[HttpPost("events/ical")]
[Produces("text/calendar")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> GenerateIcsFile([FromBody] GetIcalRequest calendarEvent)
{
    var icsContent = await calendarService.GenerateIcsFileAsync(calendarEvent);
    return File(icsContent, "text/calendar", $"{SafeFileName(calendarEvent.Title)}.ics");
}
```

### Example (Read Endpoint)

```csharp
/// <summary>
/// Returns work days within a specified date range.
/// </summary>
[Authorize]
[HttpGet("workdays")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> GetWorkDays([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
{
    var workDays = await service.GetWorkDaysAsync(startDate, endDate);
    return Ok(workDays);
}
```

When reviewing, please check:

- Is the `<summary>` comment missing?
- Are relevant `[ProducesResponseType]` attributes missing?
- Does a POST creation method use `Created()` or `CreatedAtAction()` instead of `StatusCode(StatusCodes.Status201Created, ...)`? → Violation.
- Does a file-download endpoint (e.g. `.ics`) return a link string or delegate to an external file service instead of `File(bytes, contentType, fileName)`? → Violation.
- Does the method contain a `try/catch` block? → This is a violation; error handling belongs in the service layer.
- Is the method itself still slim (no business logic in the controller)?
