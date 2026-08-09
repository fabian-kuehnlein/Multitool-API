# Repository Coding Conventions – Overview

This repository uses a two-level rule set. The instructions files in `.github/instructions/`
**must always be considered** and are authoritative for all work in this repo.

## Rule hierarchy

1. **Microsoft .NET/C# conventions** ([C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions),
   [.NET Coding Conventions](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/))
   are the baseline. Everything not explicitly overridden below follows them.
2. **The files in `.github/instructions/`** define the project-specific additions and
   exceptions. Where they differ from Microsoft conventions, they take precedence.

Precedence within `.github/instructions/`: the more specific file wins. A later file
refines an earlier one but never contradicts it.

## File index (load order)

| File | Scope |
|---|---|
| `01-general.instructions.md` | This file: rule hierarchy, project context, general review principles |
| `02-backend.instructions.md` | All production code (`**/*.cs`) – controllers, services, repositories, models |
| `03-testing.instructions.md` | All test code (`**/*Tests.cs`, `**/*.Tests/**/*.cs`, `**/*Test.cs`) |

## Project Context

This repository contains a .NET 10 / ASP.NET Core REST API (Clean Architecture) that
serves a separate Angular frontend. See `AGENTS.md` for architecture and commands.

## General Review Principles

- Unless explicitly stated otherwise, follow the common .NET/C# conventions (e.g., Microsoft C# Coding Conventions).
- Point out violations of SOLID principles, unnecessary duplication, and missing error handling in the right place.
- Prefer readability and consistency with existing code over personal style.
- Only comment when a rule from the instructions files is violated or a real risk exists (bug, security, performance) – no pure matters of taste without justification.
- Unused `using` directives must be removed to maintain a clean codebase and avoid unnecessary dependencies.
