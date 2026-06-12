# Multitool API

A RESTful Web API built with **.NET 10** and **ASP.NET Core**, serving as the backend for the Multitool Frontend (repository will be published soon).
The application follows a **Clean Architecture** pattern and currently provides the following features:
- **Authentication & Security** with JWT-based protection and rate limiting.
- **Calendar** with recurring event support.
- **Custom Table Builder** for user-defined data structures.
- **Todo List** for task management.

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

### Todo List
- Create, read, update, and delete task items
- Toggle task completion status
- Request validation for task creation and updates

### Authentication & Security
- **JWT-based authorization** protecting core API endpoints
- Secure user registration validated by an administrative key (`X-Admin-Key` header)
- **Rate limiting** configured on the login endpoint to prevent brute force attempts
- Secure password hashing using **BCrypt**

### Background Jobs
- Scheduled database cleanup of past calendar events using a customizable cron expression

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
│          Multitool.Infrastructure           │  EF Core, Repositories, External API Clients, Auth
└─────────────────────────────────────────────┘
```
---

## Tech Stack

| Category | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL |
| Object Mapping | Mapster |
| Containerization | Docker |
| Testing | xUnit, Moq, FluentAssertions, SQLite (in-memory) |
| Documentation | Swagger / OpenAPI |
| CI/CD | GitHub Actions |
| Cryptography & Auth | BCrypt.Net-Next, JWT (JSON Web Tokens) |
| Scheduling | Cronos |

---

## Project Structure

The following project structure illustrates the Clean Architecture layout and the separation of concerns across API, Application, Domain, and Infrastructure layers.

```
Multitool.sln
│
├── src/
│   ├── Multitool.Api/                  # Entry point, controllers, middleware, background jobs
│   │   ├── BackgroundJobs/
│   │   ├── Configuration/
│   │   ├── Controllers/
│   │   ├── Exceptions/
│   │   │   └── GlobalExceptionHandler.cs
│   │   └── Extensions/
│   │
│   ├── Multitool.Application/          # Services, DTOs, mappings
│   │   ├── Interfaces/
│   │   ├── Mappings/
│   │   │   └── MappingConfig.cs
│   │   ├── Models/
│   │   └── Services/
│   │
│   ├── Multitool.Domain/               # Core entities and contracts
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Exceptions/
│   │   └── Interfaces/
│   │
│   └── Multitool.Infrastructure/       # EF Core, repositories, API clients, authentication helpers
│       ├── ApiClients/
│       ├── Authentification/
│       ├── Data/
│       │   ├── Migrations/
│       │   ├── AppDbContext.cs
│       └── Repositories/
│
└── tests/
    ├── Multitool.Api.Tests/            # Controller unit tests
    ├── Multitool.Application.Tests/    # Service unit tests
    ├── Multitool.Infrastructure.Tests/ # Repository and infrastructure unit tests
    └── Multitool.Tests.Shared/         # Shared test fixtures and test data
```

---

## API Reference

All endpoints are also available via Swagger UI at `/swagger` when running in development mode.

### Authentication – `/api/auth`

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/register` | Registers a new user. Requires the `X-Admin-Key` header. |
| `POST` | `/login` | Authenticates a user and returns a JWT session token. Rate-limited by IP. |

### Calendar – `/api/calendar`

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/events` | Get events by date range; optionally filter by category IDs. Requires JWT. |
| `GET` | `/events/search` | Full-text search across event title and note. Requires JWT. |
| `POST` | `/events` | Create a new calendar event. Requires JWT. |
| `PUT` | `/events` | Update an existing calendar event. Requires JWT. |
| `DELETE` | `/events/{id}` | Delete a calendar event by ID. Requires JWT. |
| `GET` | `/holidays/{year}` | Get Bavarian public holidays for a given year. |

### Category – `/api/category`

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/categories` | Get all available event categories. Requires JWT. |

### Custom Tables – `/api/customtable`

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/tables` | Get a list of all tables. Requires JWT. |
| `GET` | `/tables/{id}` | Get a full table with columns and rows. Requires JWT. |
| `POST` | `/tables` | Create a new table with an initial column. Requires JWT. |
| `PUT` | `/tables/{id}` | Rename a table. Requires JWT. |
| `DELETE` | `/tables/{id}` | Delete a table and all its data. Requires JWT. |
| `POST` | `/tables/{tableId}/columns` | Add a new default column to a table. Requires JWT. |
| `PUT` | `/columns/{id}` | Update column name, type and order. Requires JWT. |
| `PUT` | `/columns/order` | Reorder multiple columns in bulk. Requires JWT. |
| `DELETE` | `/tables/{tableId}/columns/{columnId}` | Delete a column. Requires JWT. |
| `POST` | `/tables/{tableId}/rows` | Add a new empty row. Requires JWT. |
| `PUT` | `/rows/order` | Reorder multiple rows in bulk. Requires JWT. |
| `DELETE` | `/tables/{tableId}/rows` | Delete multiple rows by ID. Requires JWT. |
| `PUT` | `/rows/{rowId}/cells/{columnId}` | Create or update a single cell value. Requires JWT. |

### Todo List – `/api/todo`

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/` | Get all todo items. Requires JWT. |
| `POST` | `/` | Create a new todo item. Requires JWT. |
| `PUT` | `/{id}` | Update an existing todo item's details. Requires JWT. |
| `PATCH` | `/{id}/toggle` | Toggle completion status of a todo item. Requires JWT. |
| `DELETE` | `/{id}` | Delete a todo item by ID. Requires JWT. |

### Status – `/api/status`

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/live` | Liveness check endpoint. Allows anonymous access. |

---

## Environment Variables

Configure the following environment variables or configuration keys in your `appsettings.json` / container environment:

| Variable | Description |
|---|---|
| `DB_CONNECTION_STRING` | Connection string to the PostgreSQL database |
| `Jwt__Key` | Secret key used to sign JWT session tokens (required) |
| `Jwt__Issuer` | Issuer of the JWT session tokens (default: `MultitoolApi`) |
| `Jwt__Audience` | Audience of the JWT session tokens (default: `MultitoolFrontend`) |
| `AdminKey` | Key checked on user registration via `X-Admin-Key` header |
| `APPLY_MIGRATIONS` | Set to `true` to run migrations on startup in development environment |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment, e.g. `Development` or `Production` |
| `CronJobs__CleanUpPastEvents` | Cron expression for background cleanup of calendar events (default: `0 0 1 * *`) |
| `CronJobs__CleanUpPastEventsMonths` | Months of past calendar events to retain during cleanup (default: `3`) |

---

## Test coverage:

| Project | Scope |
|---|---|
| `Multitool.Api.Tests` | Controller tests — verifies HTTP responses, routing, and service delegation |
| `Multitool.Application.Tests` | Service tests — verifies business logic and repository interactions |
| `Multitool.Infrastructure.Tests` | Repository and utility tests — verifies database operations using SQLite in-memory, and checks helper logic like JWT generation |
| `Multitool.Tests.Shared` | Shared test fixtures, mock setups, and test seed data |

Tests are written with **xUnit**, assertions use **FluentAssertions**, and dependencies are mocked with **Moq**.

---

## CI/CD

A GitHub Actions workflow runs on every push and pull request to `main` and `dev`:

1. Restores NuGet dependencies
2. Builds the solution in Release configuration
3. Runs the API controller tests
4. Runs the application service tests
5. Runs the infrastructure repository tests
