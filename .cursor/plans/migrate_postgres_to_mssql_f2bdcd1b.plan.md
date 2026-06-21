---
name: Migrate Postgres to MSSQL
overview: Swap the EF Core provider in Tacdent.Data from Npgsql (PostgreSQL) to Microsoft.EntityFrameworkCore.SqlServer, regenerate the InitialCreate migration for SQL Server, and update connection strings, docker-compose (SQL Server 2022 under amd64 emulation), and docs. Entity classes and configurations stay unchanged.
todos:
  - id: pkg
    content: Swap Npgsql package for Microsoft.EntityFrameworkCore.SqlServer in Tacdent.Data.csproj
    status: completed
  - id: provider
    content: Change UseNpgsql -> UseSqlServer in DependencyInjection.cs and DesignTimeDbContextFactory.cs (with SQL Server fallback conn string)
    status: completed
  - id: connstr
    content: Update DefaultConnection in appsettings.json and appsettings.Development.json to SQL Server format
    status: completed
  - id: migration
    content: Delete Postgres InitialCreate + snapshot, regenerate InitialCreate for SQL Server
    status: completed
  - id: docker
    content: Replace postgres service with SQL Server 2022 (platform linux/amd64) in docker-compose.yml
    status: completed
  - id: docs
    content: Update README stack, quick start, and migration notes for SQL Server
    status: completed
  - id: verify
    content: Build, run, and smoke-test (services seed + appointment create/list)
    status: completed
isProject: false
---

# Migrate PostgreSQL to SQL Server (MSSQL)

## Why this is low-risk
All database concerns live in `Tacdent.Data`. The entity configs use only provider-agnostic APIs (`HasMaxLength`, `HasConversion<string>`, `HasPrecision`, `HasData`), so **no changes to `Core` entities or `Configurations/` are needed**. Only the provider package, the `Use...` calls, connection strings, the migration, and infra/docs change.

## Type mapping (handled automatically by the SqlServer provider)
- `Guid` -> `uniqueidentifier`, `string(n)` -> `nvarchar(n)`, `decimal(10,2)` -> `decimal(10,2)`
- `DateOnly` -> `date`, `TimeOnly` -> `time`, `DateTime` (audit) -> `datetime2`
- `int Id` -> `IDENTITY`, enum-as-string `Status` -> `nvarchar(20)`, `HasData` seed -> identical

## Changes

### 1. Provider package — [src/Tacdent.Data/Tacdent.Data.csproj](src/Tacdent.Data/Tacdent.Data.csproj)
Remove `Npgsql.EntityFrameworkCore.PostgreSQL`; add `Microsoft.EntityFrameworkCore.SqlServer` (10.0.x to match EF Core 10.0.9). Keep `Microsoft.EntityFrameworkCore.Relational`/`.Design`.

### 2. Runtime provider — [src/Tacdent.Data/DependencyInjection.cs](src/Tacdent.Data/DependencyInjection.cs)
Change `options.UseNpgsql(...)` to `options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))`.

### 3. Design-time provider — [src/Tacdent.Data/Context/DesignTimeDbContextFactory.cs](src/Tacdent.Data/Context/DesignTimeDbContextFactory.cs)
Change `.UseNpgsql(...)` to `.UseSqlServer(...)` and update the `TACDENT_DB` fallback to a SQL Server connection string.

### 4. Connection strings
- [src/Tacdent.Api/appsettings.json](src/Tacdent.Api/appsettings.json): replace `DefaultConnection` with
  `Server=localhost,1433;Database=tacdent;User Id=sa;Password=Tacdent_dev_2026;TrustServerCertificate=True` (Encrypt defaults on; `TrustServerCertificate=True` needed for the container's self-signed cert).
- [src/Tacdent.Api/appsettings.Development.json](src/Tacdent.Api/appsettings.Development.json): add the same `ConnectionStrings:DefaultConnection` for local dev clarity.

### 5. Regenerate the migration (chosen: fresh InitialCreate)
Delete the Postgres-specific migration files in [src/Tacdent.Data/Migrations/](src/Tacdent.Data/Migrations):
- `20260620120818_InitialCreate.cs`
- `20260620120818_InitialCreate.Designer.cs`
- `TacdentDbContextModelSnapshot.cs`

Then regenerate against SQL Server (DB auto-created/applied on API startup via existing `Database.MigrateAsync()`):
```bash
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef migrations add InitialCreate \
  --project src/Tacdent.Data --startup-project src/Tacdent.Api --output-dir Migrations
```
(Requires the SQL Server container running for design-time, or it builds the model from code without connecting.)

### 6. Local DB — [docker-compose.yml](docker-compose.yml)
Replace the `postgres` service with SQL Server 2022 under amd64 emulation (Apple Silicon):
```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    platform: linux/amd64
    container_name: tacdent-sqlserver
    restart: unless-stopped
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "Tacdent_dev_2026"
      MSSQL_PID: "Developer"
    ports:
      - "1433:1433"
    volumes:
      - tacdent_mssql_data:/var/opt/mssql
volumes:
  tacdent_mssql_data:
```
EF creates the `tacdent` database on first run, so no init script is needed.

### 7. Docs — [README.md](README.md)
Update the stack line (Npgsql -> SQL Server), Quick start (container name/port 1433, sa credentials), and the `TACDENT_DB` note to the SQL Server connection string.

## Verification
1. `docker compose up -d` (first SQL Server boot under emulation takes ~30-60s)
2. `dotnet build`
3. `dotnet run --project src/Tacdent.Api` -> migration applies, seed services inserted
4. Smoke test: `GET /api/services` returns the 5 seeded rows; create + list an appointment.

## Notes
- Production: set a strong `sa`/app-user password and real `DefaultConnection` via environment variables; don't commit prod credentials. Consider a least-privilege SQL login instead of `sa` for the app in prod.
- Audit fields stay UTC by convention (`DateTime.UtcNow` in the interceptor); `datetime2` has no offset, which matches the existing app behavior.