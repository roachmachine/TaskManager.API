# TaskManager API

A REST API for managing task-related data built with ASP.NET Core and Entity Framework Core.

## Highlights

- .NET 10 target
- Controller-based ASP.NET Core Web API
- DTO validation via DataAnnotations
- EF Core data access
- Unit tests with xUnit, FluentAssertions, Moq, and EF Core InMemory

## Repository layout

- `TaskManager.API/` — Web API project
- `TaskManager.API.Tests/` — Unit tests
- `assets/icons/` — README icon assets

## Requirements

- .NET 10 SDK

## Build

```bash
dotnet restore
dotnet build
```

## Run

```bash
dotnet run --project TaskManager.API
```

## Test

```bash
dotnet test TaskManager.API.Tests
```

## API overview

The API follows a typical REST pattern using controller actions. Example controller: `UserTypeController`.

### User types

- `GET /api/usertype` — returns a paginated list
- `GET /api/usertype/{id}` — returns a single user type
- `POST /api/usertype` — creates a new user type
- `PUT /api/usertype/{id}` — updates an existing user type
- `DELETE /api/usertype/{id}` — deletes a user type

### Validation behavior

DTOs use DataAnnotations. In unit tests that call controllers directly, model validation does not run automatically. If you assert on `BadRequest` results, validate the DTO and populate `ModelState` in the test before invoking the action.

## Configuration

Environment-specific configuration is read from `TaskManager.API` appsettings. Update connection strings and logging there as needed.

## Status

API project builds with `dotnet build`
Unit tests run with `dotnet test`

## Notes for contributors

- Keep controllers thin and delegate data access to EF Core via `TaskManagerDbContext`.
- Favor DTOs for input and output models.
- Match existing test patterns and use the InMemory database for unit tests.
