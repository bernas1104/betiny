# Betiny — Agent Guide

## Project Overview

URL shortener built in .NET 10.0 with Clean Architecture. PostgreSQL for storage, Redis for caching. Frontend planned with React.

Solution structure (BeTiny.slnx):
- `src/BeTiny.Api/` — ASP.NET Web API (entry point)
- `src/BeTiny.Application/` — use cases / CQRS
- `src/BeTiny.Domain/` — domain entities & interfaces
- `src/BeTiny.Infrastructure/` — persistence, redis, etc.
- `src/BeTiny.IOC/` — DI registration extension
- `test/BeTiny.Tests/` — xUnit tests (references all projects)

## Build / Test Commands

```bash
# Build entire solution
dotnet build

# Build a specific project
dotnet build src/BeTiny.Api/BeTiny.Api.csproj

# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run a single test class
dotnet test --filter "FullyQualifiedName~Namespace.ClassName"

# Run tests matching a trait
dotnet test --filter "Category=Unit"

# Run tests in watch mode (re-run on changes)
dotnet watch test

# Run the API locally (http://localhost:5245)
dotnet run --project src/BeTiny.Api

# Launch infra dependencies (PostgreSQL + Redis)
docker compose up -d

# Bring up everything via docker compose
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
- **`[ExcludeFromCodeCoverage]`** on DI registration, Program entry point, and other non-testable infrastructure
- **PascalCase** for classes, methods, properties, public fields
- **camelCase** for local variables, private fields
- **No BOM** on .cs files (UTF-8 without BOM preferred)
- **No comments** on implementation code unless the intent is genuinely non-obvious

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
- Test files live in `test/BeTiny.Tests/` mirroring the `src/` folder structure
- Use `dotnet test --filter` to target specific tests; no custom test runner scripts
- Coverlet is configured for code coverage

### Naming Conventions

- **Classes/Files** — PascalCase matching the type name (`UrlShortenerService.cs`)
- **Tests** — `{Class}Tests.cs` with methods named `{Method}_{Scenario}_Returns{Expected}`
- **Records** — singular nouns (`WeatherForecast`, `ShortenedUrl`)
- **Interfaces** — `I` prefix (`IUrlRepository`)
- **Async methods** — `Async` suffix (`CreateAsync`, `GetByIdAsync`)

### Error Handling

- Use the Result pattern (Success/Failure discriminated return) rather than exceptions for expected business logic failures
- Exceptions reserved for truly exceptional / infrastructure failures
- Log via `ILogger<T>` (structured logging with Serilog planned)

## Infrastructure

- **PostgreSQL** via Npgsql — service name `ngpsql` in compose.yml
- **Redis** — service name `redis` in compose.yml
- Environment variables prefixed `BETINY_NPGSQL_*` and `BETINY_REDIS_*`
- Default ports: PostgreSQL 5432, Redis 6379

## VS Code / Editor

- `.vscode/settings.json` is committed and should be updated for project-wide settings
- No `.editorconfig`, no `.cursorrules`, no Copilot instructions file currently exist
- `.vscode/` and `.idea/` in `.gitignore`
