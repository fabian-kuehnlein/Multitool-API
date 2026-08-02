# Repository Custom Instructions

## Project Context

This repository contains a .NET/C# backend.
For more specific rules, see the path-based files in `.github/instructions/`:

- `dotnet-backend.instructions.md` – General conventions for .NET backend code (controllers, services, repositories).
- `dotnet-unittests.instructions.md` – Conventions specifically for unit tests in the .NET backend.

## General Review Principles

- Unless explicitly stated otherwise, follow the common .NET/C# conventions (e.g., Microsoft C# Coding Conventions).
- Point out violations of SOLID principles, unnecessary duplication, and missing error handling in the right place.
- Prefer readability and consistency with existing code over personal style.
- Only comment when a rule from the linked instructions files is violated or a real risk exists (bug, security, performance) – no pure matters of taste without justification.
