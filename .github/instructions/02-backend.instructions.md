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
- **Create**: `CreateTodoAsync(Todo todo)` / `CreateTodoAsync(CreateTodoDto dto)`
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

```csharp
public async Task UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto)
{
    var existingTodo = await todoRepository.GetTodoByIdAsync(id)

    if (existingTodo == null)
        throw new NotFoundException($"Todo with ID {id} not found.");

    existingTodo.Title = updateTodoDto.Title;
    existingTodo.Description = updateTodoDto.Description;
    existingTodo.CategoryId = updateTodoDto.CategoryId;
    existingTodo.Priority = updateTodoDto.Priority;
    existingTodo.DueDate = updateTodoDto.DueDate;

    await todoRepository.UpdateTodoAsync(existingTodo);
}
```

## DTOs and Mapping (Mapster)

All object transformation between DTOs and entities is done with **Mapster**. There is exactly one convention
for how objects flow through the services — no mix of manual object creation and Mapster mapping.

### Record-based DTOs (Immutable)

All DTOs (Data Transfer Objects) used for data transfer (requests and responses) **must be positional records without `{ get; set; }` properties**:

```csharp
// ✅ Correct - Immutable positional record
public record CreateTodoDto(
    string Title,
    string? Description,
    int CategoryId,
    int Priority,
    DateTime? DueDate);

// ❌ Incorrect - Record or Class with { get; set; } property bodies
public record TodoDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
}
```

- Data is received as immutable DTOs from controllers and passed into services.
- In the service layer, DTOs are mapped to domain entities (or mapped back from entities to response DTOs where necessary).

### Action-specific DTOs

Every request and every response carries a dedicated DTO. The naming scheme is:

| DTO | Purpose | Contains |
|---|---|---|
| `Create<Entity>Dto` | Request body of a `POST` action | Only client-supplied fields. **Never an `Id`**, never server-managed fields. |
| `Update<Entity>Dto` | Request body of a `PUT`/`PATCH` action | Only mutable fields. **Never an `Id`** — the id comes from the route. |
| `<Entity>Dto` / purpose-specific response DTO | Response | The full serializable view, including the `Id`. |

- **`Create<Entity>Dto` has no `Id`.** The `Id` is generated by the database and set on the entity, never accepted
  from the client. The same applies to all server-managed fields (timestamps, default flags).
- **`Update<Entity>Dto` never contains an `Id` either.** The `Id` comes from the route
  (`PUT api/[controller]/[resource]/{id}`), consistently across all update routes. The route identifies the existing
  object; the body carries only the mutable fields.
- **Domain entities are never bound via `[FromBody]` and never serialized as a response.** The controller receives
  DTOs and returns DTOs.

### Mapping via Mapster

- **Creations and reads** run through Mapster. Custom mappings are registered **once** in
  `MappingConfig.cs` via `NewConfig<TSource, TDest>()` + `.Map(...)`; services only call `.Adapt<T>()`.
- Default / server-managed values are mapped in `MappingConfig.cs`, not assigned by hand in the service:

```csharp
// MappingConfig.cs
config.NewConfig<CreateTodoDto, Todo>()
    .Map(dest => dest.IsDone, src => false)
    .Map(dest => dest.CreationDateTime, src => DateTime.Now);
```

- **Updates of existing database objects use explicit property-by-property assignment**
  (`existing.Name = dto.Name;`) instead of `dto.Adapt(existing)`. The changes must be visible at a glance;
  `Adapt` on an existing entity hides which fields are being modified.
- Manual object creation (`new Entity { ... }`) in services is **not allowed for pure copying**.
  It is only acceptable where real business logic is involved (calculations, aggregations, combining multiple sources).

### Example

```csharp
public async Task<int> CreateTodoAsync(CreateTodoDto createTodoDto)
{
    var todo = dto.Adapt<Todo>();
    
    return await todoRepository.CreateTodoAsync(todo);
}
```

Update — explicit assignment so every change is visible:

```csharp
public async Task UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto)
{
    var existingTodo = await todoRepository.GetTodoByIdAsync(id)

    if (existingTodo == null)
        ?? throw new NotFoundException($"Todo with ID {id} not found");

    existingTodo.Title = updateTodoDto.Title;
    existingTodo.Description = updateTodoDto.Description;
    existingTodo.DueDate = updateTodoDto.DueDate;

    await todoRepository.UpdateTodoAsync(existingTodo);
}
```

When reviewing, please check:

- Do CRUD method names in services and repositories strictly follow `[Action][Module][Async]` (e.g. `CreateTodoAsync`, `GetTodoByIdAsync`, `UpdateTodoAsync`, `DeleteTodoAsync`)?
- Does repository update/delete accept the full entity object instead of raw primitive IDs (after service fetches and validates existence)?
- Are all DTOs positional records without `{ get; set; }` property bodies?
- Does a `Create<Entity>Dto` or `Update<Entity>Dto` contain an `Id` or another server-managed field? → Violation.
- Is a domain entity bound via `[FromBody]` or returned directly as a response? → Violation.
- Is an existing entity updated via `dto.Adapt(existing)` instead of explicit property assignment? → Violation.
- Are default / server-managed values mapped in `MappingConfig.cs` instead of in the service?

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
public async Task<IActionResult> CreateTodo([FromBody] CreateTodoDto createTodoDto)
{
    var id = await todoService.CreateTodoAsync(createTodoDto);
    return StatusCode(StatusCodes.Status201Created, id);
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
- Does the method contain a `try/catch` block? → This is a violation; error handling belongs in the service layer.
- Is the method itself still slim (no business logic in the controller)?
