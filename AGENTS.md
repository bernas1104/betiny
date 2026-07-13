# Betiny — Agent Guide

## Project Overview

URL shortener built in .NET 10.0 with Clean Architecture. PostgreSQL for storage, Redis for caching. Frontend planned with React.

Solution structure (BeTiny.slnx):
- `src/BeTiny.Api/` — ASP.NET Web API (entry point)
- `src/BeTiny.Application/` — use cases / CQRS
- `src/BeTiny.Domain/` — domain entities & interfaces
- `src/BeTiny.Infrastructure/` — persistence, redis, etc.
- `src/BeTiny.IOC/` — DI registration extension
- `test/BeTiny.UnitTests/` — xUnit unit tests (references all projects, no external dependencies)
- `test/BeTiny.IntegrationTests/` — xUnit integration tests with Testcontainers (PostgreSQL + Redis)

## Build / Test Commands

```bash
# Build entire solution
dotnet build

# Build a specific project
dotnet build src/BeTiny.Api/BeTiny.Api.csproj

# Run all tests
dotnet test

# Run unit tests only (fast, no Docker required)
dotnet test test/BeTiny.UnitTests/

# Run integration tests (requires Docker)
dotnet test test/BeTiny.IntegrationTests/

# Run tests with verbose output
dotnet test --verbosity normal

# Run a single test class
dotnet test --filter "FullyQualifiedName~Namespace.ClassName"

# Run tests in watch mode (re-run on changes)
dotnet watch test

# Generate code coverage report
# Report is generated in ./coverage/ as HTML + Cobertura XML
dotnet test --collect:"XPlat Code Coverage" --settings .runsettings

# Run the API locally (http://localhost:5245)
dotnet run --project src/BeTiny.Api

# Launch infra dependencies (PostgreSQL + Redis)
docker compose up -d

# Bring up infra dependencies in attached mode
docker compose up
```

## Commit Convention

Conventional Commits enforced via commitlint + husky:

```
feat(scope): message       # new feature
fix(scope): message        # bug fix
chore(scope): message      # tooling, config, deps
refactor(scope): message   # code change with no behavior change
test(scope): message       # adding/modifying tests
docs(scope): message       # documentation
```

Use `npm run commit` or `npm run cm` for interactive commit prompt (commitizen). Scope examples: `url-shortening`, `auth`, `analytics`, `logging`, `redis`, `cache`.

The commit-msg hook runs `npx commitlint`, and the pre-commit hook runs `dotnet test`.

## Code Style

### C# Conventions

- **File-scoped namespaces** (`namespace BeTiny.IOC;` — no braces)
- **ImplicitUsings enabled** — global `using` is automatic; only add explicit usings for external packages
- **Nullable enabled** — annotate with `?` where null is valid (`string?`)
- **var preferred** when the type is obvious (`var builder = WebApplication.CreateBuilder(args)`)
- **Primary constructors** for simple types (`record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)`)
- **Extension methods** in `public static` classes, with `this` parameter (`this IServiceCollection services`)
- **`[ExcludeFromCodeCoverage]`** on DI registration, Program entry point, controllers, base entities (`Entity<T>`, `AggregateRoot<TId, TIdType>`), `Unit`, and other non-testable infrastructure
- **PascalCase** for classes, methods, properties, public fields
- **camelCase** for local variables, private fields
- **No BOM** on .cs files (UTF-8 without BOM preferred)
- **No comments** on implementation code unless the intent is genuinely non-obvious
- **EF Core entity convention**: private parameterless constructor with `#pragma warning disable CS8618` for entities that require it

### Domain Patterns

The project follows DDD-inspired patterns with these base classes in `BeTiny.Domain.Common`:

- **`Entity<TIdType>`** — base for all entities; provides `Id` (declared `virtual` so aggregates can `override` it with their typed ID), `IsActive`, `CreatedAt`, `UpdatedAt`, `DeletedAt`
- **`AggregateRoot<TId, TIdType>`** — extends `Entity<TId>` where `TId : AggregateRootId<TIdType>`; marks an entity as an aggregate root and `override`s `Id` with the typed `TId` (instead of hiding it with `new`)
- **`ValueObject`** — base for value objects with structural equality via `GetEqualityComponents()`
- **`AggregateRootId<TIdType>`** — extends `ValueObject`; wraps the underlying ID type (`Value` property)

Concrete entities (e.g., `User : AggregateRoot<UserId, Guid>`) use a typed ID value object (e.g., `UserId : AggregateRootId<Guid>`) created via a static factory (`UserId.CreateUnique()`).

`ShortUrl` has a unified `AliasUrl` property (required in the database, max 50 chars) with an `AliasUrlType` enum (`ShortCode`, `CustomAlias`) to distinguish auto-generated short codes from user-provided custom aliases. The constructor is **private**; instances are created via the static factory methods `CreateFromShortCode(...)` and `CreateFromCustomAlias(...)` (which resolve temporal coupling by setting the alias and expiration in one step). `SetAliasUrl` is private and enforces type-specific validation rules: short codes max 7 characters, custom aliases must match `^[A-Za-z0-9_-]{3,50}$` (exposed publicly via `ShortUrl.CustomAliasRegex()`). `SetAliasUrl` accepts an optional `IReservedAliasPolicy` and throws `ReservedAliasException` when the alias is reserved. `IsExpired` and `SetExpiration` accept `IDateTimeProvider` for testability.

### CQRS & Pipeline

The project uses a **custom lightweight CQRS** implementation in `BeTiny.Application.Common.Cqrs`:

- **`ISender`** — entry point for dispatching requests (`Send<TResponse>(IRequest<TResponse>, CancellationToken)`)
- **`IPublisher`** — entry point for fire-and-forget notifications (`Publish<TNotification>(TNotification, CancellationToken)`)
- **`IRequest<TResponse>`** / **`ICommand<TResponse>`** / **`IQuery<TResponse>`** — marker interfaces for requests
- **`IRequest`** — marker for requests with no response (equivalent to `IRequest<Unit>`)
- **`IRequestHandler<TRequest, TResponse>`** — handler contract implemented by command/query handlers
- **`INotification`** — marker interface for pub/sub notifications (extends `IRequest` → `IRequest<Unit>`)
- **`Unit`** — a `readonly struct` representing a void-like response for requests with no return value
- **`INotificationHandler<TNotification>`** — handler contract for side-effect notifications
- **`IPipelineBehavior<TRequest, TResponse>`** — middleware contract for cross-cutting concerns (applies only to requests)

Registered pipeline behaviors (applied in order):
- **`ValidationBehavior`** — dynamically resolves `IValidator<>` via `IServiceProvider` and returns `Result<T>` with validation errors for requests
- **`LoggingBehavior`** — logs start and elapsed time (ms) for request execution via `ILogger<TRequest>`

Handlers, behaviors, and the publisher are registered via Scrutor assembly scanning in `ConfigureHandlers.cs`.

Notifications run all matching handlers in parallel via `Task.WhenAll` (async/await). The `Publisher` **no longer swallows exceptions**: faulted handlers' inner exceptions are collected and surfaced as an `AggregateException`, and each inner exception is logged separately via `ILogger<Publisher>`. The fire-and-forget decision is now made by the **caller** — e.g., `GetByShortUrlQuery` wraps `_publisher.Publish(...)` in a try/catch so the redirect proceeds even when click-event tracking fails.

### Repository Pattern

- **`IRepository<TEntity, TId, TIdType>`** — generic read/write contract (`AddAsync`, `GetByFilterAsync`, `Detach`)
- **`GenericRepository<TEntity, TId, TIdType>`** — EF Core implementation in Infrastructure; `AddAsync` catches Postgres unique-constraint violations (`23505` for `IX_ShortUrls_AliasUrl`) and throws `DuplicateAliasUrlException`; `Detach` sets the entity's `EntityState` to `Detached` so a failed insert can be discarded (called by `CreateShortUrlCommand` on `DuplicateAliasUrlException`)
- Domain entities are accessed through the generic interface; specialized repositories can extend it if needed

### EF Core Configuration

- Entity configurations are in `src/BeTiny.Infrastructure/Postgres/EntityConfig/` implementing `IEntityTypeConfiguration<T>`
- The `DbContext` (`BeTinyContext` in `src/BeTiny.Infrastructure/Postgres/Context/`) applies configs via `modelBuilder.ApplyConfigurationsFromAssembly`
- Typed IDs (e.g., `ClickEvent.Id`) are configured with `ValueGeneratedNever()` since values are generated by the domain, not the database
- `ClickEvent.IpAddress` is sized at 45 chars to accommodate IPv6 addresses
- Migrations live in `src/BeTiny.Infrastructure/Postgres/Migrations/`

### Project Dependencies (Layer Rules)

```
Api → IOC
IOC → Application, Infrastructure
Application → Domain
Infrastructure → Application, Domain
Domain → (no dependencies)
Tests → (all projects)
```

Do not introduce circular dependencies or upward references (e.g., Domain should never reference Infrastructure).

### Testing (xUnit)

- Use xUnit `[Fact]` for plain tests, `[Theory]` + `[InlineData]` for parameterized tests
- Test files live in `test/BeTiny.UnitTests/` and `test/BeTiny.IntegrationTests/` mirroring the `src/` folder structure
- Use `dotnet test --filter` to target specific tests; no custom test runner scripts
- Coverlet is configured for code coverage with `coverlet.msbuild` + `coverlet.collector`
- Use the Arrange-Act-Assert (AAA) test pattern
- **NSubstitute** is used for mocking dependencies; **Bogus** for fake data; **AwesomeAssertions** for assertions
- Integration tests use **Testcontainers** for real PostgreSQL + Redis containers
- A `.runsettings` file at repo root configures coverage formats (HTML + Cobertura) and 80% minimum threshold

### Naming Conventions

- **Classes/Files** — PascalCase matching the type name (`UrlShortenerService.cs`)
- **Tests** — `{Class}Test.cs` (singular) with methods named `{Method}_{Scenario}_Returns{Expected}`
- **Records** — singular nouns (`WeatherForecast`, `AliasUrl`)
- **Interfaces** — `I` prefix (`IUrlRepository`)
- **Async methods** — `Async` suffix (`CreateAsync`, `GetByIdAsync`)

### Error Handling

- Use the Result pattern (`Result<T>` with `Error` records) rather than exceptions for expected business logic failures
- `Result<T>.Errors` is **non-nullable** and defaults to `Array.Empty<Error>()`; `IsSuccess` is `Errors.Count == 0`
- The base `Controller` class provides `HandleResult<T>(Result<T>, Func<IActionResult>)` to map results to `IActionResult` (including `ProblemDetails` for failures)
  - `ValidationError` → 400 Bad Request
  - `NotFoundError` → 404 Not Found
  - `ExpiredError` → 410 Gone
  - `ConflictError` → 409 Conflict
  - A failed result with no errors → 500 Internal Server Error (fallback `ProblemDetails`)
- Exceptions reserved for truly exceptional / infrastructure failures
- Log via `ILogger<T>` (structured logging with Serilog planned)

## Infrastructure

- **PostgreSQL** via Npgsql — service name `ngpsql` in compose.yml
- **Redis** — service name `redis` in compose.yml
- Environment variables prefixed `BETINY_NPGSQL_*` and `BETINY_REDIS_*`
- Default ports: PostgreSQL 5432, Redis 6379
- DbContext registration (`BeTinyContext`) is configured via `IOC/DependencyInjections/ConfigureDatabases.cs` using `AddDbContext` + `UseNpgsql`
- Connection strings come from `appsettings.Development.json` under `ConnectionStrings:Postgres` and `ConnectionStrings:Redis`

## Services

- **`IIpResolver`** / **`IpResolver`** — resolves country from IP address via an external IP geolocation API (ip-api.com) using `IIpApi`; checks the `IpApiResponse.Success` flag and falls back to `"Unknown"` on failure or missing IP (logging the API's `Message`)
- **`IDeviceDetector`** / **`DeviceDetector`** — parses User-Agent strings via **UAParser** to classify devices as `Desktop`, `Mobile`, `Tablet`, or `Unknown`
- **`IDateTimeProvider`** / **`DateTimeProvider`** — provides `DateTime.UtcNow` abstraction for testability
- **`IReservedAliasPolicy`** / **`ReservedAliasPolicy`** — checks whether an alias is reserved, combining built-in defaults (`ReservedAliasDefaults`: `admin`, `login`, `dashboard`, `api`, `auth`, `health`, `swagger`, `docs`) with configurable entries (`ReservedAliasOptions` from `appsettings.json`); matching is case-insensitive. Used by `CreateShortUrlRequestValidator`, `CreateShortUrlCommand`, and `ShortUrl.SetAliasUrl`

## IOC Registration

- `ConfigureDatabases.cs` — registers `DbContext` and Redis connections
- `ConfigureHandlers.cs` — scans and registers `IRequestHandler<>`, `INotificationHandler<>`, `ISender`, `IPublisher`, and pipeline behaviors
- `ConfigureOptions.cs` — registers options from `appsettings.json`, including `IpApiOptions` and `ReservedAliasOptions` (validates each configured alias against `ShortUrl.CustomAliasRegex()` at startup)
- `ConfigureServices.cs` — registers domain services (`IShortCodeGenerator`, `IIpResolver`, `IDeviceDetector`, `IDateTimeProvider`, `IReservedAliasPolicy` / `ReservedAliasPolicy`, `ReservedAliasDefaults`) and the Refit client for `IIpApi`
- `ConfigureValidators.cs` — registers FluentValidation validators from the Application assembly

## VS Code / Editor

- No `.vscode/settings.json` currently committed
- No `.editorconfig`, no `.cursorrules`, no Copilot instructions file currently exist
- `.vscode/` and `.idea/` in `.gitignore`

## Technical Refinements

After a technical refinement is completed, upload it to the respective Github Issue. It should be appended to the issues' description, not added as a issue comment.

## Updates

After any changes to the project, update, if necessary, the `README.md` and `AGENTS.md` files.
