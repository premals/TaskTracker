# TaskTracker API

TaskTracker is a minimal ASP.NET Core Web API for managing task items with SQLite persistence, FluentValidation-based business validation, consistent `Result` responses, Swagger documentation, and unit tests.

## Features

- CRUD endpoints for `TaskItem`
- SQLite persistence through Entity Framework Core
- FluentValidation for input and business validation
- Standardized API responses through a `Result` class
- Global exception handling with safe production error messages
- Swagger/OpenAPI support
- Unit tests for validation, business rules, and happy paths

## TaskItem Model

A task contains the following fields:

- `Id`
- `Title` required, maximum 100 characters
- `Description` optional
- `Status` one of `Todo`, `InProgress`, `Done`
- `DueDate` optional

Business rule:

- A task cannot be marked as `Done` if the title is empty or whitespace

## API Endpoints

- `POST /tasks`
- `GET /tasks`
- `GET /tasks/{id}`
- `PUT /tasks/{id}`
- `DELETE /tasks/{id}`

Swagger UI is available at `/swagger`.

## Code Architecture

The project uses a simple layered structure so each responsibility stays in one place and the code remains easy to extend.

### 1. Entry Point

`Program.cs` wires up the application:

- registers EF Core with SQLite
- registers FluentValidation validators
- registers the generic repository and task service
- enables Swagger
- enables the global exception handler
- creates the database automatically on startup using `EnsureCreated()`

Why we use it:

- keeps startup concerns centralized
- makes dependency injection explicit
- makes the app easy to run locally with minimal setup

### 2. Endpoints Layer

Files in `Endpoints/` define the HTTP routes.

Responsibilities:

- expose the `/tasks` routes
- accept request payloads
- call the service layer
- convert `Result` objects into HTTP responses with the correct status codes

Why we use it:

- keeps HTTP concerns separate from business rules
- makes the route handlers small and easy to read

### 3. Service Layer

Files in `Services/` contain the application logic.

Responsibilities:

- coordinate validation
- apply the task business rules
- map request data into entities
- call the repository
- return success and failure outcomes through the `Result` class

Why we use it:

- prevents business logic from leaking into endpoints
- gives one clear place to test behavior
- makes later changes easier, for example adding authorization or more rules

### 4. Validation Layer

Files in `Validators/` contain FluentValidation rules and parsing helpers.

Responsibilities:

- validate required fields
- validate title length
- validate allowed status values
- validate `DueDate` format
- enforce the rule that `Done` requires a non-empty title

Why we use FluentValidation:

- rule definitions stay declarative and centralized
- business validation is easier to read than hand-written `if` blocks scattered across services
- validation failures are easier to evolve as requirements grow
- it integrates cleanly with dependency injection

### 5. Repository Layer

Files in `Repositories/` implement a generic repository on top of EF Core.

Responsibilities:

- read and write entities
- abstract direct DbContext access away from the service layer

Why we use it:

- keeps persistence details out of services
- makes unit testing easier because the service depends on an abstraction
- creates a reusable data access pattern for future entities

### 6. Data Layer

Files in `Data/` contain the EF Core `DbContext` and database initialization.

Responsibilities:

- define the `TaskItems` table mapping
- enforce database-level shape such as required title and max length
- initialize the SQLite database

Why we use EF Core with SQLite:

- EF Core gives a productive ORM for CRUD scenarios
- SQLite is lightweight and ideal for local development and small APIs
- the database can live as a local file without external infrastructure

### 7. Common Response and Error Handling

Files in `Common/` define `Result`, `Error`, and helper extensions.

Responsibilities:

- standardize success and failure responses
- keep validation errors and business validation errors in one predictable format
- map validation output into API-friendly error payloads

Why we use a `Result` class:

- callers get a consistent response structure
- business validation does not need to throw exceptions
- endpoints can map outcomes to HTTP status codes in a uniform way

### 8. Global Exception Handling

`Exceptions/GlobalExceptionHandler.cs` handles unexpected failures and malformed request bodies.

Responsibilities:

- return `400 Bad Request` for invalid request body shapes and malformed JSON values
- return `500 Internal Server Error` for unknown exceptions
- hide internal exception details in production

Why we use it:

- avoids repeating `try/catch` in every endpoint
- keeps production responses safe
- provides a single place to control error behavior

## Validation and Error Flow

Validation happens before data is saved:

1. The endpoint receives the request.
2. The service runs FluentValidation.
3. Validation failures are converted into the project `Result` format.
4. Endpoint helpers translate those results into HTTP status codes.

Current status code behavior:

- `201 Created` for successful task creation
- `200 OK` for successful reads, updates, and deletes
- `400 Bad Request` for validation failures and malformed request bodies
- `404 Not Found` when a task id does not exist
- `500 Internal Server Error` for unexpected failures

## Test Coverage Explanation

The tests live in `TaskTracker.Tests/TaskServiceTests.cs` and focus on service-level behavior.

What is covered:

- validation failure when title is whitespace
- validation failure when title exceeds 100 characters
- validation failure when `DueDate` format is invalid
- business rule failure when a task is marked `Done` with an empty or whitespace title
- one successful create path
- one successful update path

Why these tests are useful:

- they verify both basic validation and business validation
- they keep the core rules fast to test without depending on HTTP or SQLite
- they confirm the service produces the expected `Result` outcomes

Current test approach:

- uses a lightweight in-memory fake repository
- tests the service layer in isolation
- keeps the test suite fast and deterministic

What is not covered yet:

- endpoint integration tests
- database integration tests against SQLite
- Swagger or startup configuration tests

## How To Run

### Prerequisites

- .NET SDK 10.0 or later installed

### 1. Restore dependencies

```bash
dotnet restore
```

### 2. Run the API

```bash
dotnet run --project TaskTracker.csproj
```

Default local URLs from `launchSettings.json`:

- `http://localhost:5248`
- `https://localhost:7270`

Swagger UI:

- `http://localhost:5248/swagger`
- `https://localhost:7270/swagger`

### 3. Run the tests

```bash
dotnet test TaskTracker.slnx
```

### 4. Build the solution

```bash
dotnet build TaskTracker.slnx
```

## Database Notes

- In Development, the app uses `tasktracker.development.db`
- In other environments, the app uses `tasktracker.db`
- The database file is created automatically on first run

## Example Request Payload

```json
{
  "title": "Finish documentation",
  "description": "Add architecture notes to README",
  "status": "InProgress",
  "dueDate": "2026-04-10"
}
```

## Summary Of Why This Design Was Chosen

- Minimal API keeps the app lightweight
- Services keep business logic out of HTTP handlers
- FluentValidation keeps validation readable and maintainable
- `Result` keeps response handling consistent
- Generic repository keeps data access reusable and testable
- Global exception handling keeps error behavior centralized and safe
- Unit tests protect the most important business behavior
