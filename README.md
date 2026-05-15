# BeTiny

A URL shortener built as a system design exercise using **.NET 10** with Clean Architecture.

## Architecture

```
src/
├── BeTiny.Api          — ASP.NET Web API (entry point)
├── BeTiny.Application  — Use cases / CQRS
├── BeTiny.Domain       — Domain entities & interfaces
├── BeTiny.Infrastructure — Persistence, Redis, etc.
├── BeTiny.IOC          — DI registration extension
test/
└── BeTiny.Tests        — xUnit tests
```

### Layer rules

```
Api → IOC → Application, Infrastructure
Application → Domain
Infrastructure → Application, Domain
Domain → (no dependencies)
Tests → (all projects)
```

## Tech stack

| Component      | Technology                              |
|----------------|-----------------------------------------|
| Runtime        | .NET 10.0                               |
| Database       | PostgreSQL 16 (via Npgsql + EF Core)    |
| Cache          | Redis 7                                 |
| Testing        | xUnit + Coverlet                        |
| Commit hooks   | Husky + Commitlint (conventional commits) |

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://docker.com) (for PostgreSQL + Redis)

## Getting started

```bash
# 1. Clone the repository
git clone <repo-url>
cd betiny

# 2. Set up environment variables
cp .env .env.example  # .env is already present with defaults

# 3. Start infrastructure (PostgreSQL + Redis)
docker compose up -d

# 4. Run the API
dotnet run --project src/BeTiny.Api

# The API is available at http://localhost:5245
```

## Configuration

Environment variables are prefixed with `BETINY_NPGSQL_*` and `BETINY_REDIS_*`:

| Variable                     | Default         | Description            |
|------------------------------|-----------------|------------------------|
| `BETINY_NPGSQL_USER`         | `betiny`        | PostgreSQL user        |
| `BETINY_NPGSQL_PASSWORD`     | `betiny_secret` | PostgreSQL password    |
| `BETINY_NPGSQL_DATABASE`     | `betiny`        | PostgreSQL database    |
| `BETINY_NPGSQL_PORT`         | `5432`          | PostgreSQL port        |
| `BETINY_REDIS_PASSWORD`      | `redis_secret`  | Redis password         |
| `BETINY_REDIS_PORT`          | `6379`          | Redis port             |

Connection strings are set in `appsettings.Development.json` under `ConnectionStrings:Postgres` and `ConnectionStrings:Redis`.

## Commands

```bash
dotnet build                    # Build the solution
dotnet test                     # Run all tests
dotnet test --filter "Category=Unit"  # Filter tests by trait
dotnet watch test               # Re-run tests on changes
dotnet run --project src/BeTiny.Api   # Run the API
docker compose up -d            # Start PostgreSQL + Redis only
docker compose up               # Start everything (future)
```

## Database migrations

This project uses **EF Core migrations** (with Npgsql for PostgreSQL). The `DbContext` (`BeTinyContext`) lives in `src/BeTiny.Infrastructure`.

```bash
# Create a new migration
dotnet ef migrations add <Name> \
  --project src/BeTiny.Infrastructure \
  --startup-project src/BeTiny.Api

# Apply pending migrations to the database
dotnet ef database update \
  --project src/BeTiny.Infrastructure \
  --startup-project src/BeTiny.Api

# Revert the last migration (removes it from the project)
dotnet ef migrations remove \
  --project src/BeTiny.Infrastructure \
  --startup-project src/BeTiny.Api

# Revert to a specific migration
dotnet ef database update <PreviousMigrationName> \
  --project src/BeTiny.Infrastructure \
  --startup-project src/BeTiny.Api

# List all migrations
dotnet ef migrations list \
  --project src/BeTiny.Infrastructure \
  --startup-project src/BeTiny.Api

# Generate a SQL script
dotnet ef migrations script \
  --project src/BeTiny.Infrastructure \
  --startup-project src/BeTiny.Api
```

> Make sure PostgreSQL is running (`docker compose up -d`) before running `database update`.

## Commit convention

This project enforces [Conventional Commits](https://www.conventionalcommits.org/) via Husky + Commitlint.

```bash
npm run commit    # Interactive commit prompt (Commitizen)
npm run cm        # Shorthand for the above
```

Format: `type(scope): message`

- `feat(url-shortening)` — new feature
- `fix(redis)` — bug fix
- `chore(deps)` — tooling, config, dependencies
- `refactor(auth)` — code change with no behavior change
- `test(core)` — adding/modifying tests
- `docs(readme)` — documentation

## License

MIT &mdash; see [LICENSE](LICENSE).
