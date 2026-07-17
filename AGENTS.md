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

- **`Entity<TIdType>`** — base for all entities; provides `Id` (declared `virtual` so aggregates can `override` it with their typed ID), `IsActive`, `CreatedAt`, `UpdatedAt`, `DeletedAt`; exposes `internal Deactivate()` and `internal Delete()` (set `IsActive = false` / `DeletedAt = now` and stamp `UpdatedAt`) — these are `internal` so only the domain + tests can call them, enabled by `InternalsVisibleTo("BeTiny.UnitTests")` in `BeTiny.Domain.csproj`
- **`AggregateRoot<TId, TIdType>`** — extends `Entity<TId>` where `TId : AggregateRootId<TIdType>`; marks an entity as an aggregate root and `override`s `Id` with the typed `TId` (instead of hiding it with `new`)
- **`ValueObject`** — base for value objects with structural equality via `GetEqualityComponents()`
- **`AggregateRootId<TIdType>`** — extends `ValueObject`; wraps the underlying ID type (`Value` property)

Concrete entities (e.g., `User : AggregateRoot<UserId, Guid>`) use a typed ID value object (e.g., `UserId : AggregateRootId<Guid>`) created via a static factory (`UserId.CreateUnique()`).

`ShortUrl` has a unified `AliasUrl` property (required in the database, max 50 chars) with an `AliasUrlType` enum (`ShortCode`, `CustomAlias`) to distinguish auto-generated short codes from user-provided custom aliases. The constructor is **private**; instances are created via the static factory methods `CreateFromShortCode(...)` and `CreateFromCustomAlias(...)` (which resolve temporal coupling by setting the alias and expiration in one step). `SetAliasUrl` is private and enforces type-specific validation rules: short codes max 7 characters, custom aliases must match `^[A-Za-z0-9_-]{3,50}$` (exposed publicly via `ShortUrl.CustomAliasRegex()`). `SetAliasUrl` accepts an optional `IReservedAliasPolicy` and throws `ReservedAliasException` when the alias is reserved. `IsExpired` and `SetExpiration` accept `IDateTimeProvider` for testability. `SetUserId(UserId?)` assigns ownership post-factory (called by `CreateShortUrlCommand` when the request is authenticated); it throws `ArgumentNullException` if the argument is null and `InvalidOperationException` if `UserId` is already set (one-time-assignment invariant); anonymous creation leaves `UserId = null`. `ShortUrl.UserId` is a nullable `UserId?` (optional ownership).

`Email` is a `sealed partial` value object (`: ValueObject`) and the single source of truth for a valid + normalized email. `Email.Create` trims, lowercases (`ToLowerInvariant`), enforces max 254 chars (RFC 5321), and validates via `Email.EmailRegex()` (`^[^@\s]+@[^@\s]+\.[^@\s]+$`, exposed via `[GeneratedRegex]`). Equality is structural on the normalized `Value`, so `Email.Create("A@X.com") == Email.Create("a@x.com")` — this makes email uniqueness case-insensitive end-to-end. `partial` is required by the `[GeneratedRegex]` source generator. `Email.RedactedValue` exposes a logging-safe form (e.g. `b***@domain.com`) used by `LoginCommand`.

`User` follows the same private-ctor + static-factory pattern as `ShortUrl`: the public `User(string email)` constructor was removed (it left `PasswordHash = string.Empty`, an invariant-violating half-built entity) and replaced by `User.Create(Email email, string password, IPasswordHasher passwordHasher)`. `Create` hashes the password **inside** the entity via the `IPasswordHasher` port (the invariant "a `User` never holds plaintext" lives in the domain), enforces password rules via `User.PasswordRegex()` (`^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,72}$`: min 8, max 72, ≥1 upper / ≥1 lower / ≥1 digit, exposed via `[GeneratedRegex]`), sets `Plan = Plans.Free`, `IsActive = true`, and `CreatedAt = DateTime.UtcNow` (raw — no `IDateTimeProvider`, mirroring the `ShortUrl` ctor). `User.Email` is typed as the `Email` VO (not `string`). `User` is `sealed partial` for the `[GeneratedRegex]` source generator.

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
- **`GenericRepository<TEntity, TId, TIdType>`** — EF Core implementation in Infrastructure; `AddAsync` catches Postgres unique-constraint violations (`23505`) and maps the constraint name to a dedicated exception: `IX_ShortUrls_AliasUrl` → `DuplicateAliasUrlException`, `IX_Users_Email` → `DuplicateEmailException`; `Detach` sets the entity's `EntityState` to `Detached` so a failed insert can be discarded (called by `CreateShortUrlCommand` on `DuplicateAliasUrlException` and by `RegisterCommand` on `DuplicateEmailException`)
- Domain entities are accessed through the generic interface; specialized repositories can extend it if needed

### EF Core Configuration

- Entity configurations are in `src/BeTiny.Infrastructure/Postgres/EntityConfig/` implementing `IEntityTypeConfiguration<T>`
- The `DbContext` (`BeTinyContext` in `src/BeTiny.Infrastructure/Postgres/Context/`) applies configs via `modelBuilder.ApplyConfigurationsFromAssembly`
- Typed IDs (e.g., `ClickEvent.Id`) are configured with `ValueGeneratedNever()` since values are generated by the domain, not the database
- `ClickEvent.IpAddress` is sized at 45 chars to accommodate IPv6 addresses
- `UserEntityConfig` configures `Email` with a `HasConversion` (writes `Email.Value`, reads `Email.Create(v)`) so the `Email` VO is transparent to EF; the column is sized at 254 chars (RFC 5321) and has a unique index `IX_Users_Email` as the DB-level backstop for duplicate emails
- `ShortUrlEntityConfig` configures a nullable FK from `ShortUrl.UserId` → `User.Id` via `HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull).IsRequired(false)` — no navigation property on either side (DDD aggregate-root boundary); deleting a user nullifies `UserId` on their short URLs (preserves public URLs created by now-deleted users). An index `IX_ShortUrls_UserId` is created by convention for the FK column
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
  - `UnauthorizedError` → 401 Unauthorized
  - `ForbiddenError` → 403 Forbidden
  - `NotFoundError` → 404 Not Found
  - `ConflictError` → 409 Conflict
  - `ExpiredError` → 410 Gone
  - A failed result with no errors → 500 Internal Server Error (fallback `ProblemDetails`)
- Exceptions reserved for truly exceptional / infrastructure failures
- Dedicated domain exceptions map specific infrastructure failures to clean HTTP responses: `ReservedAliasException` (caught by `CreateShortUrlCommand` → 400), `DuplicateAliasUrlException` and `DuplicateEmailException` (caught by their handlers → 409)
- JwtBearer middleware returns **401 Unauthorized** directly for missing/expired/invalid-signature tokens before the handler runs — no `ErrorType` or `Result` is involved. `[Authorize]` on an action enforces authentication; actions without `[Authorize]` (and without a class-level `[Authorize]`) are anonymous by default. `UseAuthentication()` → `UseAuthorization()` → `MapControllers()` pipeline order in `Program.cs` is required for `[Authorize]` to be enforced; `UseAuthentication` populates `HttpContext.User` if a valid token is present even on anonymous endpoints (enables optional auth on `POST /api/v1/urlshortener`)
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
- **`IPasswordHasher`** / **`PasswordHasher`** — hashes and verifies passwords using **BCrypt** (via `BCrypt.Net-Next`, work factor `12`, using the enhanced modes `EnhancedHashPassword` / `EnhancedVerify`); the `IPasswordHasher` port lives in Domain (so `User.Create` can hash inside the entity), the impl + package live in Infrastructure. `VerifyPassword` catches `BCrypt.Net.SaltParseException` and returns `false` (so a malformed hash never throws). `DummyPasswordHash` is a precomputed hash of `"dummy"` used by `LoginCommand` to keep verification time roughly constant when the user is not found (timing-attack mitigation). Used by `User.Create` (domain), `RegisterCommand`, and `LoginCommand`
- **`ITokenProvider`** / **`JwtTokenProvider`** — issues JWTs (HMAC SHA256 via `System.IdentityModel.Tokens.Jwt`) for authenticated users; `IssueToken(Guid userId, string email)` returns a `TokenResult(Token, ExpiresAt)`. Configured through `JwtOptions` (`Issuer`, `Audience`, `SigningKey`, `ExpiryMinutes`) and uses `IDateTimeProvider` for the issued-at / expiration timestamps so expiry is testable. The port lives in Application (`BeTiny.Application.Common.Interfaces.Services`), the impl + package live in Infrastructure. Used by `LoginCommand`
- **`ICurrentUser`** / **`CurrentUser`** — provides the authenticated user context from `HttpContext.User` claims: `UserId?` (parsed from the `sub` claim via `Guid.TryParse` → `UserId.Create`), `string? Email` (raw `email` claim), `Plans? Plan` (`Enum.TryParse<Plans>` on the `plan` claim, null on invalid), `bool IsAuthenticated`. The port lives in Domain (`BeTiny.Domain.Common.Interfaces`), the impl lives in Infrastructure (`BeTiny.Infrastructure.Services`, `sealed class`) and requires `<FrameworkReference Include="Microsoft.AspNetCore.App" />` in the Infrastructure csproj for `IHttpContextAccessor`; registered as Scoped. Properties are computed on each access (no caching); `MapInboundClaims = false` in JwtBearer means original claim names (`sub`, `email`, `plan`) are read directly. Used by `CreateShortUrlCommand` (optional auth — null `UserId` = anonymous) and `GetMeQuery` (required auth — `UserId` non-null is guaranteed by `[Authorize]`; throws `InvalidOperationException` if null, signaling a claim-configuration bug)

## IOC Registration

- `ConfigureDatabases.cs` — registers `DbContext` and Redis connections
- `ConfigureHandlers.cs` — scans and registers `IRequestHandler<>`, `INotificationHandler<>`, `ISender`, `IPublisher`, and pipeline behaviors
- `ConfigureOptions.cs` — registers options from `appsettings.json`, including `IpApiOptions`, `ReservedAliasOptions` (validates each configured alias against `ShortUrl.CustomAliasRegex()` at startup), and `JwtOptions` (validates non-empty `Issuer`/`Audience`/`SigningKey`, a `SigningKey` of at least 32 UTF-8 bytes, and `ExpiryMinutes > 0` at startup)
- `ConfigureServices.cs` — registers domain services (`IShortCodeGenerator`, `IIpResolver`, `IDeviceDetector`, `IDateTimeProvider`, `IReservedAliasPolicy` / `ReservedAliasPolicy`, `ReservedAliasDefaults`, `IPasswordHasher` / `PasswordHasher`, `ITokenProvider` / `JwtTokenProvider`) and the Refit client for `IIpApi`
- `ConfigureValidators.cs` — registers FluentValidation validators from the Application assembly
- `ConfigureAuthentication.cs` — registers `IHttpContextAccessor` (via `AddHttpContextAccessor()`), `ICurrentUser` → `CurrentUser` (Scoped), JwtBearer authentication (`AddAuthentication` + `AddJwtBearer` with `MapInboundClaims = false`, `ClockSkew = TimeSpan.Zero`, issuer/audience/lifetime/issuer-signing-key validation), and `AddAuthorization()`. Reads `JwtOptions` from the `Jwt` config section. Called from `RegisterBindings` as the last registration step.

## VS Code / Editor

- No `.vscode/settings.json` currently committed
- No `.editorconfig`, no `.cursorrules`, no Copilot instructions file currently exist
- `.vscode/` and `.idea/` in `.gitignore`

## Technical Refinements

After a technical refinement is completed, upload it to the respective Github Issue. It should be appended to the issues' description, not added as a issue comment.

Also, save the file to the @.plans/ folder as a .md file with the following format: `technical-refinement-issue-<issue-number>.md`. If in doubt, read the folder for examples.

## Updates

After any changes to the project, update, if necessary, the `README.md` and `AGENTS.md` files.
