# Multitool API

A RESTful Web API built with **.NET 10** and **ASP.NET Core**, serving as the backend for the Multitool Frontend (repository will be published soon).
The application follows a **Clean Architecture** pattern and currently provides the following features:
- **Calendar** with recurring event support.
- **Custom Table Builder** for user-defined data structures.

---

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [API Reference](#api-reference)
- [Environment Variables](#environment-variables)
- [Test coverage](#test-coverage)
- [CI/CD](#cicd)

---

## Features

### Calendar
- Create, read, update and delete calendar events
- Support for **recurring events** via iCalendar `RRULE` strings (daily, weekly, monthly, yearly)
- Date-range-based event retrieval with optional **category filtering**
- Full-text **event search** across title and notes
- **German public holidays** fetched from a third-party API ([api-feiertage.de](https://get.api-feiertage.de))
- Category management with custom colors

### Custom Table Builder
- Dynamically create tables with user-defined columns
- Five supported column types: `String`, `Int`, `Decimal`, `Date`, `Bool`
- Add, remove and reorder columns and rows at runtime
- Inline cell editing with automatic type coercion and validation
- Persistent row and column ordering

---

## Architecture

The solution follows a **Clean Architecture** approach with a strict separation of concerns across four layers:

```
┌─────────────────────────────────────────────┐
│               Multitool.Api                 │  Controllers, Exception Handling, DI Setup
├─────────────────────────────────────────────┤
│           Multitool.Application             │  Business Logic, Services, DTOs, Mappings
├─────────────────────────────────────────────┤
│             Multitool.Domain                │  Entities, Interfaces, Enums, Exceptions
├─────────────────────────────────────────────┤
│          Multitool.Infrastructure           │  EF Core, Repositories, External API Clients
└─────────────────────────────────────────────┘
```
---

## Tech Stack

| Category | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core |
| ORM | Entity Framework Core 9 |
| Database | MariaDB 10.11 |
| Object Mapping | Mapster |
| Containerization | Docker / Docker Compose |
| Testing | xUnit, Moq, FluentAssertions |
| Documentation | Swagger / OpenAPI |
| CI/CD | GitHub Actions |

---

## Project Structure

```
Multitool.sln
│
├── src/
│   ├── Multitool.Api/                  # Entry point, controllers, middleware
│   │   ├── Controllers/
│   │   │   ├── CalendarController.cs
│   │   │   └── CustomTableController.cs
│   │   ├── Exceptions/
│   │   │   └── GlobalExceptionHandler.cs
│   │   └── Extensions/
│   │       └── MigrationExtensions.cs
│   │
│   ├── Multitool.Application/          # Services, DTOs, mappings
│   │   ├── Interfaces/
│   │   ├── Mappings/
│   │   │   └── MappingConfig.cs
│   │   ├── Models/
│   │   └── Services/
│   │       ├── CalendarService.cs
│   │       └── CustomTableService.cs
│   │
│   ├── Multitool.Domain/               # Core entities and contracts
│   │   ├── Entities/
│   │   │   ├── Calendar/
│   │   │   └── CustomTable/
│   │   ├── Enums/
│   │   ├── Exceptions/
│   │   └── Interfaces/
│   │
│   └── Multitool.Infrastructure/       # EF Core, repositories, API clients
│       ├── ApiClients/
│       │   └── CalendarApiClient.cs
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   └── Migrations/
│       └── Repositories/
│           ├── CalendarRepository.cs
│           └── CustomTableRepository.cs
│
└── tests/
    ├── Multitool.Api.Tests/            # Controller unit tests
    ├── Multitool.Application.Tests/    # Service unit tests
    └── Multitool.Tests.Shared/         # Shared test fixtures and data
```

---

## API Reference

All endpoints are also available via Swagger UI at `/swagger` when running in development mode.

### Calendar – `/api/calendar`

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/events` | Get events by date range; optionally filter by category IDs |
| `GET` | `/events/search` | Full-text search across event title and note |
| `POST` | `/events` | Create a new calendar event |
| `PUT` | `/events` | Update an existing calendar event |
| `DELETE` | `/events/{id}` | Delete a calendar event by ID |
| `GET` | `/categories` | Get all available event categories |
| `GET` | `/holidays/{year}` | Get Bavarian public holidays for a given year |

### Custom Tables – `/api/customtable`

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/tables` | Get a list of all tables |
| `GET` | `/tables/{id}` | Get a full table with columns and rows |
| `POST` | `/tables` | Create a new table with an initial column |
| `PUT` | `/tables/{id}` | Rename a table |
| `DELETE` | `/tables/{id}` | Delete a table and all its data |
| `POST` | `/tables/{tableId}/columns` | Add a new column to a table |
| `PUT` | `/columns/{id}` | Update column name, type and order |
| `PUT` | `/columns/order` | Reorder multiple columns in bulk |
| `DELETE` | `/tables/{tableId}/columns/{columnId}` | Delete a column |
| `POST` | `/tables/{tableId}/rows` | Add a new row |
| `PUT` | `/rows/order` | Reorder multiple rows in bulk |
| `DELETE` | `/tables/{tableId}/rows` | Delete multiple rows by ID |
| `PUT` | `/rows/{rowId}/cells/{columnId}` | Create or update a single cell value |

---

## Environment Variables

Copy the example file and fill in your values:

| Variable | Description |
|---|---|
| `DB_HOST` | Database host (use `db` for Docker Compose) |
| `DB_PORT` | Database port (default: `3306`) |
| `DB_ROOT_PASSWORD` | MariaDB root password |
| `DB_NAME` | Database name |
| `DB_USER` | Database user |
| `DB_PASSWORD` | Database password |
| `API_PORT` | Port the API is exposed on |
| `ASPNETCORE_ENVIRONMENT` | `Development` or `Production` |

## Test coverage:

| Project | Scope |
|---|---|
| `Multitool.Api.Tests` | Controller tests — verifies HTTP responses and service delegation |
| `Multitool.Application.Tests` | Service tests — verifies business logic and repository interactions |
| `Multitool.Tests.Shared` | Shared test fixtures and seed data |

Tests are written with **xUnit**, assertions use **FluentAssertions**, and dependencies are mocked with **Moq**.

---

## CI/CD

A GitHub Actions workflow runs on every push and pull request to `main` and `dev`:

1. Restores NuGet dependencies
2. Builds the solution in Release configuration
3. Runs the API controller tests
4. Runs the application service tests
