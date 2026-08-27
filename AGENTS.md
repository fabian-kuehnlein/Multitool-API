# AGENTS.md

.NET 10 / ASP.NET Core REST API (Clean Architecture) for the Multitool app. PostgreSQL via EF Core, Mapster mappings, iCal.NET (`.ics` export in `CalendarService`), xUnit tests. Backend of a separate Angular frontend.

## Auto-loaded rules

The files in `.github/instructions/` are injected into every session and **must always be considered and applied** (the Microsoft .NET/C# conventions are the baseline; the instructions files define the project-specific deviations and take precedence):

- `01-general.instructions.md` — rule hierarchy, project context, general review principles.
- `02-backend.instructions.md` — authoritative for all backend code: no `try/catch` in controllers, exceptions only in services, `ProducesResponseType`, slim controllers.
- `03-testing.instructions.md` — authoritative for all unit tests: naming, AAA, separator comments, expected tests per layer.

Follow them; don't duplicate them here.

## Architecture

4 layers, dependency direction is one-way: `Api → Application → Domain` and `Infrastructure → Application, Domain`.

- `src/Multitool.Api` — entry point (`Program.cs`), controllers, `GlobalExceptionHandler`, cron jobs.
- `src/Multitool.Application` — services (`Services/`), service interfaces (`Interfaces/`), DTOs (`Models/`), Mapster config (`Mappings/MappingConfig.cs`). Registered via `Setup.AddApplication()`.
- `src/Multitool.Domain` — entities, enums, `Domain/Exceptions/`, and **all repository interfaces** (`Interfaces/`). Has an empty `Setup.cs`.
- `src/Multitool.Infrastructure` — EF Core (`Data/AppDbContext.cs`), repository impls (`Repositories/`), auth helpers, external API client. Registered via `Setup.AddInfrastructure(connectionString)`.

Non-obvious quirks:
- DTOs and service interfaces live in `Application`, but repository contracts live in `Domain`. Don't "fix" or relocate this.
- EF mapping config lives entirely in `AppDbContext.OnModelCreating`: snake_case names, schemas `public` (default), `custom`, `worktime`; all `DateTime` columns are `timestamp without time zone` (wall-clock `ValueConverter`).
- Enums are serialized to **camelCase strings** over the wire (`JsonStringEnumConverter(JsonNamingPolicy.CamelCase)` in `Program.cs`).

## Commands

Requires .NET SDK 10 (`dotnet --version` should show 10.x).

```powershell
dotnet build ./Multitool.sln -c Release

# Tests run per project (mirrors CI) — no cross-project test runner
dotnet test tests/Multitool.Api.Tests/Multitool.Api.Tests.csproj -c Release --no-build
dotnet test tests/Multitool.Application.Tests/Multitool.Application.Tests.csproj -c Release --no-build
dotnet test tests/Multitool.Infrastructure.Tests/Multitool.Infrastructure.Tests.csproj -c Release --no-build
```

- `tests/Multitool.Tests.Shared` is a plain class library (shared test data), not a test project — don't run it as one.
- Focus a single test with `dotnet test <proj> -c Release --no-build --filter <FullyQualifiedName~Name>`.
- CI (`.github/workflows/ci.yml`) builds Release then runs the three test projects separately. Renovate (not Dependabot) handles NuGet bumps; `.github/disabled.dependabot.yml` is intentionally disabled.

## Migrations

- Migrations live in `src/Multitool.Infrastructure/Data/Migrations`; create them with `dotnet ef migrations add <Name>` from the Infrastructure project.
- `AppDbContextFactory.cs` is the design-time factory EF tooling uses. It contains a **hardcoded live Supabase connection string**, so EF tooling may try to reach that database.
- At runtime the connection string comes from `ConnectionStrings:DefaultConnection` / `DB_CONNECTION_STRING` env var (`Setup.AddInfrastructure` throws if missing).
- `Program.cs` auto-applies pending migrations **only in Production** (`app.ApplyMigrations()`).

## Secrets & config

- `src/Multitool.Api/appsettings.Development.json` and `AppDbContextFactory.cs` contain **real committed dev credentials** (Supabase DB, JWT key, AdminKey). Never copy them into docs, logs, tests, or commit new secrets.
- `AdminKey` is checked against the `X-Admin-Key` header on registration. Login is rate-limited (5/min per IP, `login-limit` policy).
- Swagger is enabled only in Development (`/swagger`). Background cron jobs (`CleanUpPastEventsCronJob`, `CleanUpPastTodosCronJob`) run every minute in dev config and purge old data.
