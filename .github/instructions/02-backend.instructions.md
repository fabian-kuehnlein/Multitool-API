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

## Exceptions only in the service layer

Exceptions should be thrown **exclusively in the service layer**.
Repositories deliver raw data and throw **no business exceptions** such as `NotFoundException`,
`ConflictException`, or validation errors.

Instead of throwing an exception in the repository, the service checks **before the actual repository call**
whether the operation is valid.

### Example

```csharp
public async Task DeleteEventAsync(int id)
{
    var exists = await calendarRepository.GetByIdAsync(id);

    if (exists == null)
        throw new NotFoundException($"Event with Id {id} not found");

    await calendarRepository.DeleteEventAsync(id);
}
```

## Controllers

Every action method in a controller should be "decorated" as follows:

1. **XML documentation comment** (`/// <summary>`) with a short, understandable description of what the endpoint does.
2. Relevant attributes such as `[Authorize]` (if applicable).
3. HTTP method attribute with route, e.g., `[HttpGet("workdays")]`.
4. `[Produces("application/json")]`.
5. `[ProducesResponseType(...)]` for every realistically possible status code (at least the success case
   and `500 Internal Server Error`; where applicable also `400`, `401`, `404` etc., if these can actually occur).
6. The method itself stays slim: call the service, return the result. No `try/catch`.

### Example

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
- Does the method contain a `try/catch` block? → This is a violation; error handling belongs in the service layer.
- Is the method itself still slim (no business logic in the controller)?
