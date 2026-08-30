# Student Management API

A minimal ASP.NET Core API for managing student records, built with a **Vertical Slice Architecture** and a lightweight **CQRS** pattern.

## Technology Stack

| Concern | Technology |
|---|---|
| Runtime / Framework | .NET 10, ASP.NET Core Minimal APIs |
| API documentation | `Microsoft.AspNetCore.OpenApi` (OpenAPI document generation) + `Swashbuckle.AspNetCore.SwaggerUI` (Swagger UI) |
| Persistence | In-memory repository (`ConcurrentDictionary`) — swappable behind `IStudentRepository` |
| CQRS | Custom, dependency-free `ICommandHandler<TCommand,TResponse>` / `IQueryHandler<TQuery,TResponse>` abstractions (no MediatR) |
| Unit testing | NUnit 4 |
| Mocking | NSubstitute |
| Integration / automation testing | `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory<Program>`) |

## Solution Structure

```
StudentManagement.sln
├── src/
│   └── StudentManagement.Api/                # ASP.NET Core minimal API host
│       ├── Program.cs                        # DI registration, middleware, endpoint mapping
│       ├── Common/
│       │   ├── Result.cs                     # Success/failure wrapper returned by command handlers
│       │   └── Cqrs/
│       │       ├── ICommandHandler.cs         # Command handler abstraction
│       │       └── IQueryHandler.cs           # Query handler abstraction
│       └── Features/
│           └── Students/                      # Everything related to the "Students" domain
│               ├── Student.cs                 # Domain entity
│               ├── StudentResponse.cs         # Shared response DTO
│               ├── IStudentRepository.cs      # Persistence abstraction
│               ├── InMemoryStudentRepository.cs
│               ├── CreateStudent/              # --- Vertical slice: Create ---
│               │   ├── CreateStudentRequest.cs # HTTP request body DTO
│               │   ├── CreateStudentCommand.cs # CQRS command
│               │   ├── CreateStudentHandler.cs # Validation + persistence logic
│               │   └── CreateStudentEndpoint.cs# Minimal API route mapping
│               ├── GetStudentById/             # --- Vertical slice: Get by id ---
│               │   ├── GetStudentByIdQuery.cs
│               │   ├── GetStudentByIdHandler.cs
│               │   └── GetStudentByIdEndpoint.cs
│               └── GetStudents/                # --- Vertical slice: Get all ---
│                   ├── GetStudentsQuery.cs
│                   ├── GetStudentsHandler.cs
│                   └── GetStudentsEndpoint.cs
└── tests/
    └── StudentManagement.Api.Tests/           # NUnit test project
        ├── Features/Students/                  # Handler unit tests (repository mocked with NSubstitute)
        │   ├── CreateStudent/CreateStudentHandlerTests.cs
        │   ├── GetStudentById/GetStudentByIdHandlerTests.cs
        │   └── GetStudents/GetStudentsHandlerTests.cs
        ├── Endpoints/                           # HTTP-level tests with a mocked repository
        │   ├── CreateStudentEndpointTests.cs
        │   └── GetStudentEndpointTests.cs
        └── Automation/                          # End-to-end tests, real repository, no mocks
            └── StudentApiAutomationTests.cs
```

## Vertical Slice Architecture

Instead of organizing code by technical layer (`Controllers/`, `Services/`, `Repositories/`), this project organizes code by **feature**. Each use case ("slice") under `Features/Students/` is self-contained and owns everything it needs end-to-end:

- Its **request/command/query** shape
- Its **handler** (business + validation logic)
- Its **endpoint** (HTTP route mapping)

```
Features/Students/CreateStudent/
├── CreateStudentRequest.cs   → what the client sends
├── CreateStudentCommand.cs   → what the handler receives
├── CreateStudentHandler.cs   → what actually happens
└── CreateStudentEndpoint.cs  → how it's exposed over HTTP
```

**Why vertical slices?**
- Adding a new feature (e.g. `UpdateStudent`, `DeleteStudent`) means adding a new folder, not touching shared controllers/services used by unrelated features.
- Each slice can evolve independently — a slice's DTO shape, validation, and persistence needs don't leak into other slices.
- Related code lives together, making the feature easy to find, read, and delete as a unit.

Only truly cross-cutting concerns live outside `Features/`:
- `Common/Cqrs/` — the generic command/query handler interfaces used by every slice.
- `Common/Result.cs` — the generic success/failure wrapper used by command handlers.
- `Features/Students/Student.cs`, `IStudentRepository.cs` — the shared domain entity and persistence contract (multiple slices in the same feature area read/write the same entity).

### CQRS

Each slice is a **Command** (writes, e.g. `CreateStudentCommand`) or a **Query** (reads, e.g. `GetStudentByIdQuery`, `GetStudentsQuery`), handled by exactly one handler:

```csharp
public interface ICommandHandler<in TCommand, TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
```

Minimal API endpoints resolve the relevant handler directly from DI as a parameter (no mediator/dispatcher library, e.g. MediatR, is used — avoiding its licensing and keeping the pipeline simple and explicit):

```csharp
app.MapPost("/api/students", async (
    CreateStudentRequest request,
    ICommandHandler<CreateStudentCommand, Result<StudentResponse>> handler,
    CancellationToken cancellationToken) => { ... });
```

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/students` | Create a new student. Returns `201 Created` + the created student, or `400 Bad Request` with validation errors. |
| `GET` | `/api/students/{id}` | Get a single student by id. Returns `200 OK` or `404 Not Found`. |
| `GET` | `/api/students` | Get all students. Returns `200 OK` with a list. |

## Running the API

```bash
dotnet run --project src/StudentManagement.Api
```

In the `Development` environment, the OpenAPI document is available at `/openapi/v1.json` and Swagger UI at `/swagger`.

## Running the Tests

```bash
dotnet test
```

Tests are layered to match the architecture:
- **Handler unit tests** — exercise a single slice's handler in isolation with `IStudentRepository` mocked via NSubstitute.
- **Endpoint tests** — exercise the HTTP pipeline through `WebApplicationFactory<Program>` with `IStudentRepository` mocked, verifying status codes and payloads.
- **Automation (end-to-end) tests** — exercise the full stack through `WebApplicationFactory<Program>` with the real `InMemoryStudentRepository`, no mocks.
