# TacDent Backend

.NET 10 Web API for appointment and service management, organized as an **n-tier solution** with SQL Server. Pairs with the [tacdent-frontend](https://github.com/dogancignakli/tacdent-frontend) Next.js site.

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
| Api | `Controllers/`, `ViewModels/`, `Validators/`, `Factories/`, `Extensions/`, `Filters/`, `Auth/` | Thin controllers; FluentValidation runs in a filter |

### Key patterns

- **Generic repository + Unit of Work** — repositories never call `SaveChanges`; the `IUnitOfWork` commits once per use case.
- **Entity-specific repositories** — e.g. `IAppointmentRepository.GetAllAsync(status)` for filtered queries.
- **Audit fields** — every entity inherits `AuditableEntity` (`CreatedAt`, `UpdatedAt`). The `AuditableEntityInterceptor` stamps them automatically on `SaveChanges`, so services never set them by hand.
- **Result pattern** — services return `Result` / `Result<T>`; `ResultExtensions` maps `ErrorType` to HTTP status codes (Validation→400, NotFound→404, Conflict→409, Unauthorized→401).
- **Mapperly** — compile-time source-generated mapping (no runtime reflection) in the Application layer.
- **FluentValidation** — validators live in the Api layer and run via `FluentValidationFilter` before controller actions.
- **Manual factories** — `IAppointmentFactory` / `IServiceFactory` convert ViewModels ↔ Core DTOs at the HTTP boundary.
- **JWT authentication** — staff log in with a shared admin password; protected endpoints require a `Bearer` token.

## Quick start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for local SQL Server)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### 1. Start SQL Server

```bash
docker compose up -d
```

On Apple Silicon, the container runs under `linux/amd64` emulation — first boot can take 30–60 seconds.

| Setting | Value |
|---------|-------|
| Server | `localhost,1433` |
| Database | `tacdent` (created automatically on first API run) |
| User | `sa` |
| Password | `Tacdent_dev_2026` |

If you previously ran the PostgreSQL container from an older compose file, clean it up with:

```bash
docker compose up -d --remove-orphans
```

### 2. Run the API

```bash
dotnet run --project src/Tacdent.Api
```

The API runs at [http://localhost:5065](http://localhost:5065). Migrations are applied automatically on startup.

### 3. Run the frontend (optional)

```bash
cd ../tacdent-frontend
cp .env.local.example .env.local   # NEXT_PUBLIC_API_URL=http://localhost:5065
npm install && npm run dev
```

The site runs at [http://localhost:3000](http://localhost:3000). Public booking is at `/appointments`; staff panel at `/admin/login`.

### API documentation (Scalar)

In **Development**, interactive API docs are served by [Scalar](https://scalar.com) (not Swagger UI):

| URL | Purpose |
|-----|---------|
| [http://localhost:5065/scalar](http://localhost:5065/scalar) | Scalar UI — browse and try endpoints |
| [http://localhost:5065/openapi/v1.json](http://localhost:5065/openapi/v1.json) | Raw OpenAPI JSON |

Scalar is only enabled when `ASPNETCORE_ENVIRONMENT=Development` (default in `launchSettings.json`).

## Authentication

Staff access uses a **shared admin password** exchanged for a JWT. Patient booking stays anonymous.

| Setting | Location | Dev value |
|---------|----------|-----------|
| Admin password | `Auth:AdminPassword` | `tacdent-admin-dev` (in `appsettings.Development.json`) |
| JWT signing key | `Jwt:Key` | set in `appsettings.Development.json` |
| Token lifetime | `Jwt:ExpiryMinutes` | `480` (8 hours) |

**Login** — `POST /api/auth/login` with `{ "password": "..." }` returns `{ "token": "...", "expiresAt": "..." }`.

**Protected requests** — send `Authorization: Bearer <token>` on appointment management endpoints.

For production, set strong `Auth:AdminPassword` and `Jwt:Key` via environment variables or user secrets. Do not commit real credentials.

## API endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/services` | — | List active dental services |
| POST | `/api/auth/login` | — | Staff login (returns JWT) |
| POST | `/api/appointments` | — | Create an appointment request (public) |
| GET | `/api/appointments` | Bearer | List appointments (optional `?status=Pending`) |
| GET | `/api/appointments/{id}` | Bearer | Get one appointment |
| PATCH | `/api/appointments/{id}/status` | Bearer | Update status |
| DELETE | `/api/appointments/{id}` | Bearer | Delete appointment |

JSON is **camelCase**. Enums are **strings** (`Pending`, `Confirmed`, `Cancelled`, `Completed`). Dates are `"YYYY-MM-DD"`; times are `"HH:mm"` or `"HH:mm:ss"`.

### Example: create appointment (public)

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

### Example: staff login + list appointments

```bash
# Login
TOKEN=$(curl -s -X POST http://localhost:5065/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"password":"tacdent-admin-dev"}' | jq -r .token)

# List all pending requests
curl -s http://localhost:5065/api/appointments?status=Pending \
  -H "Authorization: Bearer $TOKEN"
```

Validation failures return `400` with `{ "message": "Validation failed.", "errors": { ... } }`.
Domain failures (e.g. not found) return the matching status with `{ "code": "...", "message": "..." }`.

## Configuration

| Section | Purpose |
|---------|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Cors:Origins` | Allowed frontend origins (default `http://localhost:3000`) |
| `Auth:AdminPassword` | Shared staff password |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key`, `Jwt:ExpiryMinutes` | JWT token settings |

Dev overrides live in `appsettings.Development.json`. Base `appsettings.json` leaves secrets empty — fill them per environment.

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

Override the design-time connection string with the `TACDENT_DB` environment variable if needed:

```
Server=localhost,1433;Database=tacdent;User Id=sa;Password=Tacdent_dev_2026;TrustServerCertificate=True
```

## Stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10 + SQL Server
- JWT Bearer authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- Riok.Mapperly (source-generated mapping)
- FluentValidation
- Scalar.AspNetCore (API docs UI)
