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
- **Entity-specific repositories** — e.g. `IAppointmentRepository.GetPagedAsync(query)` for filtered, sorted, paginated queries at the datasource.
- **Audit fields** — every entity inherits `AuditableEntity` (`CreatedAt`, `UpdatedAt`). The `AuditableEntityInterceptor` stamps them automatically on `SaveChanges`, so services never set them by hand.
- **Result pattern** — services return `Result` / `Result<T>`; `ResultExtensions` maps `ErrorType` to HTTP status codes (Validation→400, NotFound→404, Conflict→409, Unauthorized→401).
- **Mapperly** — compile-time source-generated mapping (no runtime reflection) in the Application layer.
- **FluentValidation** — validators live in the Api layer and run via `FluentValidationFilter` before controller actions.
- **Manual factories** — `IAppointmentFactory` / `IServiceFactory` convert ViewModels ↔ Core DTOs at the HTTP boundary.
- **JWT authentication** — staff log in with email + password; JWT carries role claims (`Admin`, `Staff`). Protected endpoints require a `Bearer` token.

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

### 2. Set local dev secrets (required before first run)

Secrets are **not** in git. The API refuses to start without `Auth:AdminPassword`, a `Jwt:Key` of at least 32 characters, and (when `Recaptcha:Enabled` is `true`) `Recaptcha:SecretKey`.

```bash
dotnet user-secrets set "Auth:AdminPassword" "<your-dev-password>" --project src/Tacdent.Api
dotnet user-secrets set "Jwt:Key" "<your-32+-char-signing-key>" --project src/Tacdent.Api
dotnet user-secrets set "Recaptcha:SecretKey" "<your-recaptcha-secret-key>" --project src/Tacdent.Api
# or disable reCAPTCHA locally:
dotnet user-secrets set "Recaptcha:Enabled" "false" --project src/Tacdent.Api
# production only — must match frontend INTERNAL_API_KEY:
# dotnet user-secrets set "InternalApi:Key" "<shared-secret>" --project src/Tacdent.Api
```

Copy `src/Tacdent.Api/appsettings.Development.json` from a teammate or create one locally for the SQL Server connection string (file is gitignored).

### 3. Run the API

```bash
dotnet run --project src/Tacdent.Api --launch-profile http
```

The API runs at [http://localhost:5065](http://localhost:5065). Migrations are applied automatically on startup.

**VS Code / Cursor:** press **F5** with the `.vscode/launch.json` config — it uses the `http` profile on port `5065`.

### 4. Run the frontend (optional)

```bash
cd ../tacdent-frontend
cp .env.local.example .env.local   # set NEXT_PUBLIC_API_URL, SITE_URL, RECAPTCHA_SITE_KEY
npm install && npm run dev
```

The site runs at [http://localhost:3000](http://localhost:3000) (redirects to `/tr`). Public booking is at `/tr/appointments`; staff panel at `/tr/admin/login`.

Frontend env vars (see `tacdent-frontend` README): `NEXT_PUBLIC_API_URL`, `API_URL`, `NEXT_PUBLIC_SITE_URL`, `NEXT_PUBLIC_RECAPTCHA_SITE_KEY`, and in production `INTERNAL_API_KEY` (must match `InternalApi:Key` on the API). When reCAPTCHA is enabled on the API, the frontend site key must match the same Google reCAPTCHA v3 project.

### API documentation (Scalar)

In **Development**, interactive API docs are served by [Scalar](https://scalar.com) (not Swagger UI):

| URL | Purpose |
|-----|---------|
| [http://localhost:5065/scalar](http://localhost:5065/scalar) | Scalar UI — browse and try endpoints |
| [http://localhost:5065/openapi/v1.json](http://localhost:5065/openapi/v1.json) | Raw OpenAPI JSON |

Scalar is only enabled when `ASPNETCORE_ENVIRONMENT=Development` (default in `launchSettings.json`).

## Authentication

Staff access uses **per-user accounts** (email + password) stored in SQL Server. On first run, if the users table is empty, `AdminSeeder` creates an initial admin from `Auth:AdminEmail` / `Auth:AdminPassword` (default email `admin@tacdent.local`). Patient booking stays anonymous.

| Setting | Location | Dev setup |
|---------|----------|-----------|
| Bootstrap admin email | `Auth:AdminEmail` | Optional; defaults to `admin@tacdent.local` |
| Bootstrap admin password | `Auth:AdminPassword` | .NET user-secrets (see below) |
| JWT signing key | `Jwt:Key` | .NET user-secrets (min 32 chars) |
| Token lifetime | `Jwt:ExpiryMinutes` | `480` (8 hours) in `appsettings.Development.json` |
| reCAPTCHA secret | `Recaptcha:SecretKey` | .NET user-secrets (required when `Recaptcha:Enabled` is `true`) |
| reCAPTCHA min score | `Recaptcha:MinScore` | `0.5` in `appsettings.json` |
| reCAPTCHA enabled | `Recaptcha:Enabled` | `true` in `appsettings.json`; set `false` to skip verification locally |

**Local dev secrets** — `appsettings.Development.json` is gitignored. Set secrets once per machine:

```bash
dotnet user-secrets set "Auth:AdminPassword" "<your-dev-password>" --project src/Tacdent.Api
dotnet user-secrets set "Jwt:Key" "<your-32+-char-signing-key>" --project src/Tacdent.Api
# reCAPTCHA (public booking + login) — get keys at https://www.google.com/recaptcha/admin/create (v3)
dotnet user-secrets set "Recaptcha:SecretKey" "<your-recaptcha-secret-key>" --project src/Tacdent.Api
# or disable locally:
dotnet user-secrets set "Recaptcha:Enabled" "false" --project src/Tacdent.Api
# optional:
dotnet user-secrets set "Auth:AdminEmail" "admin@tacdent.local" --project src/Tacdent.Api
```

**Login** — `POST /api/auth/login` with `{ "email": "...", "password": "...", "recaptchaToken": "..." }` returns `{ "token": "...", "expiresAt": "...", "role": "Admin" | "Staff" }`. Rate limited to **5 attempts per IP per 5 minutes** (`429` when exceeded).

**reCAPTCHA v3** — public booking (`POST /api/appointments` via the Next.js BFF in production) and `POST /api/auth/login` require a `recaptchaToken`. The backend verifies it with Google (`success`, matching `action`, `score >= MinScore`). When `Recaptcha:Enabled` is `false` or `SecretKey` is empty, verification is skipped (validator returns success). The API refuses to start when `Enabled=true` and `SecretKey` is missing.

**BFF-only public writes (production)** — when `InternalApi:Key` is set, direct `POST /api/appointments` and `POST /api/auth/login` must include `X-Internal-Api-Key` with the same value. The Next.js BFF adds this header automatically. Leave the key empty in local dev to skip the check.

**Client IP for KVKK** — on booking, the API records the visitor IP on each `Consent` row. IP is taken from `Connection.RemoteIpAddress` after `UseForwardedHeaders` processes headers from **trusted proxies only** (`ForwardedHeaders:KnownProxies`). The BFF forwards the visitor IP; the API does not trust raw client-supplied `X-Forwarded-For` from arbitrary callers.

**Booking rate limit** — `POST /api/appointments` is limited to **5 requests per IP per 15 minutes** (`429` when exceeded), in addition to reCAPTCHA.

**Protected requests** — send `Authorization: Bearer <token>` on management endpoints. Role-restricted actions use `[Authorize(Roles = Roles.Admin)]` (e.g. delete appointments, user management, assignee updates).

The Next.js frontend stores the JWT in an **httpOnly cookie** via its BFF (`/api/auth/login`); direct API clients still use the `Bearer` header.

For production, set strong `Auth:AdminPassword` and `Jwt:Key` via environment variables or user secrets. Do not commit real credentials.

## API endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/services` | — | List active dental services |
| GET | `/api/services/all` | Admin | List all services (active + inactive) |
| POST | `/api/services` | Admin | Create service |
| PUT | `/api/services/{id}` | Admin | Update service |
| DELETE | `/api/services/{id}` | Admin | Delete service |
| GET | `/api/testimonials` | — | List active testimonials |
| GET | `/api/testimonials/all` | Admin | List all testimonials |
| POST | `/api/testimonials` | Admin | Create testimonial |
| PUT | `/api/testimonials/{id}` | Admin | Update testimonial |
| DELETE | `/api/testimonials/{id}` | Admin | Delete testimonial |
| POST | `/api/auth/login` | — | Staff login (returns JWT + role) |
| POST | `/api/appointments` | — | Create an appointment request (public; via BFF in production) |
| GET | `/api/appointments` | Bearer | Paginated list — see query params below |
| GET | `/api/appointments/{id}` | Bearer | Get one appointment |
| PATCH | `/api/appointments/{id}/status` | Bearer | Update status |
| PATCH | `/api/appointments/{id}/assignee` | Admin | Assign or clear assignee |
| DELETE | `/api/appointments/{id}` | Admin | Delete appointment |
| GET | `/api/users` | Admin | List staff accounts |
| POST | `/api/users` | Admin | Create staff account |
| PATCH | `/api/users/{id}/role` | Admin | Change user role |
| PATCH | `/api/users/{id}/status` | Admin | Activate / deactivate user |
| POST | `/api/users/{id}/password` | Admin | Reset user password |

**Appointment list query params** (all optional except defaults):

| Param | Default | Values |
|-------|---------|--------|
| `status` | — | `Pending`, `Confirmed`, `Cancelled`, `Completed` |
| `page` | `1` | `>= 1` |
| `pageSize` | `20` | `1`–`100` |
| `sortBy` | `PreferredDate` | `PreferredDate`, `CreatedAt`, `Status` |
| `sortDirection` | `Desc` | `Asc`, `Desc` |

**Paged response shape:**

```json
{
  "items": [ /* AppointmentResponse[] */ ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

JSON is **camelCase**. Enums are **strings** (`Pending`, `Confirmed`, `Cancelled`, `Completed`). Dates are `"YYYY-MM-DD"`; times are `"HH:mm"` or `"HH:mm:ss"`.

### Example: create appointment (public)

In production, call through the Next.js BFF (`POST http://localhost:3000/api/appointments`) or include `X-Internal-Api-Key` when calling the API directly.

```bash
curl -X POST http://localhost:5065/api/appointments \
  -H "Content-Type: application/json" \
  -d '{
    "patientName": "Jane Doe",
    "email": "jane@example.com",
    "phone": "+15551234567",
    "preferredDate": "2026-07-01",
    "preferredTime": "10:30",
    "serviceId": 1,
    "notes": "First visit",
    "kvkkInformationAccepted": true,
    "kvkkInformationVersion": "2026-07-05",
    "kvkkExplicitConsentAccepted": true,
    "kvkkExplicitConsentVersion": "2026-07-05",
    "recaptchaToken": "<token>"
  }'
```

On success, the API persists the appointment plus two `Consent` rows (privacy notice + explicit consent) with patient snapshot fields and client IP.

### Example: staff login + list appointments

```bash
# Login (first-run default email: admin@tacdent.local)
TOKEN=$(curl -s -X POST http://localhost:5065/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@tacdent.local","password":"<your-dev-password>"}' | jq -r .token)

# List pending requests (page 1, sorted by preferred date desc)
curl -s "http://localhost:5065/api/appointments?status=Pending&page=1&pageSize=20&sortBy=PreferredDate&sortDirection=Desc" \
  -H "Authorization: Bearer $TOKEN"
```

Validation failures return `400` with `{ "message": "Validation failed.", "errors": { ... } }`.
Domain failures (e.g. not found) return the matching status with `{ "code": "...", "message": "..." }`.

## Security

| Measure | Where |
|---------|--------|
| JWT on management endpoints | Controllers default to `[Authorize]`; public `POST` booking/login only |
| Role-based authorization | `Admin` vs `Staff` — delete, assignee, CMS, and `/api/users` are Admin-only |
| Login rate limiting | 5 attempts / IP / 5 min on `POST /api/auth/login` (`429`) |
| Booking rate limiting | 5 requests / IP / 15 min on `POST /api/appointments` (`429`) |
| BFF shared secret | `InternalApi:Key` + `X-Internal-Api-Key` on public writes when key is configured |
| Trusted proxy IPs only | `ForwardedHeaders:KnownProxies` — real client IP for KVKK without header spoofing |
| KVKK consent audit trail | `Consent` table: type, text version, accepted-at, patient snapshot, IP |
| Constant-time password check | `AuthService` |
| Fail-fast on weak/missing secrets | `Program.cs` (blank password or `Jwt:Key` &lt; 32 chars) |
| Security response headers | `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` |
| HSTS | Enabled outside Development |
| Secrets not in git | `appsettings.Development.json` gitignored; use user-secrets locally, env vars in production |

For production, also use HTTPS termination at a reverse proxy, set `Cors:Origins` to your real frontend URL, set matching `InternalApi:Key` / `INTERNAL_API_KEY`, add your BFF/nginx IP to `ForwardedHeaders:KnownProxies`, and use a least-privilege SQL login (not `sa`).

## Configuration

| Section | Purpose |
|---------|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Cors:Origins` | Allowed frontend origins (default `http://localhost:3000`) |
| `Auth:AdminEmail` | Bootstrap admin email (first run only; default `admin@tacdent.local`) |
| `Auth:AdminPassword` | Bootstrap admin password (first run only) |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key`, `Jwt:ExpiryMinutes` | JWT token settings |
| `Recaptcha:SecretKey`, `Recaptcha:MinScore`, `Recaptcha:Enabled` | reCAPTCHA v3 verification for booking + login |
| `InternalApi:Key` | Shared secret for BFF → API public writes. Empty = disabled (local dev). Set in production. |
| `ForwardedHeaders:KnownProxies` | IP addresses of trusted reverse proxies / BFF (default `127.0.0.1`, `::1`) |

Dev non-secret settings live in `appsettings.Development.json` (gitignored). Base `appsettings.json` leaves secrets empty — use user-secrets locally or environment variables in production.

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
