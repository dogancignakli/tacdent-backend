# TacDent Backend

.NET 10 Web API for appointment and service management, organized as an **n-tier solution** with PostgreSQL.

## Architecture

```
tacdent-backend/
├── Tacdent.slnx
├── docker-compose.yml
└── src/
    ├── Tacdent.Core/          # Entities, DTOs, Result<T> — no external dependencies
    ├── Tacdent.Data/          # DbContext, EF configs, audit interceptor, repositories, Unit of Work
    ├── Tacdent.Application/   # Services (business rules), Mapperly mappers
    └── Tacdent.Api/           # Controllers, ViewModels, Validators, Factories (presentation)
```

### Dependency direction

```
Api ──> Application ──> Data ──> Core
 └────────────────────────────> Core
```

- **Core** depends on nothing.
- **Data** depends on Core. Holds the `DbContext`, repository abstractions/implementations, and migrations.
- **Application** depends on Data + Core. Holds use-case services; never touches `DbContext` directly.
- **Api** depends on Application + Core. Holds controllers, HTTP ViewModels, validators, and factories. Does **not** reference Data.

### Layer responsibilities

| Layer | Folder highlights | Notes |
|-------|-------------------|-------|
| Core | `Entities/`, `Entities/Common/AuditableEntity.cs`, `DTOs/`, `Results/` | `Result<T>` + `Error` model expected failures without exceptions |
| Data | `Context/`, `Configurations/`, `Interceptors/`, `Repositories/` | Generic `Repository<T>` + entity-specific repos behind `IUnitOfWork` |
| Application | `Services/`, `Mapping/`, `Errors/` | Services return `Result<T>`; Mapperly maps entities ↔ DTOs |
| Api | `Controllers/`, `ViewModels/`, `Validators/`, `Factories/`, `Extensions/`, `Filters/` | Thin controllers; FluentValidation runs in a filter |

### Key patterns

- **Generic repository + Unit of Work** — repositories never call `SaveChanges`; the `IUnitOfWork` commits once per use case.
- **Entity-specific repositories** — e.g. `IAppointmentRepository.GetAllAsync(status)` for filtered queries.
- **Audit fields** — every entity inherits `AuditableEntity` (`CreatedAt`, `UpdatedAt`). The `AuditableEntityInterceptor` stamps them automatically on `SaveChanges`, so services never set them by hand.
- **Result pattern** — services return `Result` / `Result<T>`; `ResultExtensions` maps `ErrorType` to HTTP status codes (Validation→400, NotFound→404, Conflict→409).
- **Mapperly** — compile-time source-generated mapping (no runtime reflection) in the Application layer.
- **FluentValidation** — validators live in the Api layer and run via `FluentValidationFilter` before controller actions.
- **Manual factories** — `IAppointmentFactory` / `IServiceFactory` convert ViewModels ↔ Core DTOs at the HTTP boundary.

## Quick start

### 1. Start PostgreSQL

```bash
docker compose up -d
```

| Setting | Value |
|---------|-------|
| Host | `localhost` |
| Port | `5432` |
| Database | `tacdent` |
| User | `tacdent` |
| Password | `tacdent_dev` |

### 2. Run the API

```bash
dotnet run --project src/Tacdent.Api
```

The API runs at [http://localhost:5065](http://localhost:5065). Migrations are applied automatically on startup.

### API documentation (Scalar)

In **Development**, interactive API docs are served by [Scalar](https://scalar.com) (not Swagger UI):

| URL | Purpose |
|-----|---------|
| [http://localhost:5065/scalar](http://localhost:5065/scalar) | Scalar UI — browse and try endpoints |
| [http://localhost:5065/openapi/v1.json](http://localhost:5065/openapi/v1.json) | Raw OpenAPI JSON |

Scalar is only enabled when `ASPNETCORE_ENVIRONMENT=Development` (default in `launchSettings.json`).

## API endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/services` | List active dental services |
| GET | `/api/appointments` | List appointments (optional `?status=Pending`) |
| GET | `/api/appointments/{id}` | Get one appointment |
| POST | `/api/appointments` | Create an appointment request |
| PATCH | `/api/appointments/{id}/status` | Update status |
| DELETE | `/api/appointments/{id}` | Delete appointment |

### Example: create appointment

```bash
curl -X POST http://localhost:5065/api/appointments \
  -H "Content-Type: application/json" \
  -d '{
    "patientName": "Jane Doe",
    "email": "jane@example.com",
    "phone": "+15551234567",
    "preferredDate": "2026-07-01",
    "preferredTime": "10:30",
    "serviceType": "General Checkup",
    "notes": "First visit"
  }'
```

Validation failures return `400` with `{ "message": "Validation failed.", "errors": { ... } }`.
Domain failures (e.g. not found) return the matching status with `{ "code": "...", "message": "..." }`.

## Migrations

EF tooling targets the Data project with the Api project as startup. A `DesignTimeDbContextFactory`
lets the tools build the context without running the host.

```bash
export PATH="$PATH:$HOME/.dotnet/tools"

# add a migration
dotnet ef migrations add <Name> --project src/Tacdent.Data --startup-project src/Tacdent.Api --output-dir Migrations

# apply migrations manually (otherwise applied on API startup)
dotnet ef database update --project src/Tacdent.Data --startup-project src/Tacdent.Api
```

Override the design-time connection string with the `TACDENT_DB` environment variable if needed.

> **Note:** The schema now includes `CreatedAt` and `UpdatedAt` on all entities. If you previously
> created the `tacdent` database with the old single-project schema, drop and recreate it (or run the
> migration against a fresh database) so the new audit columns are applied.

## Stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10 + Npgsql (PostgreSQL)
- Riok.Mapperly (source-generated mapping)
- FluentValidation
- Scalar.AspNetCore (API docs UI)
```
