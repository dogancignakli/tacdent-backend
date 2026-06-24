---
name: Multi-user admin auth
overview: "Replace the single shared admin password with a real multi-user store: a User entity with per-user salted password hashes (PBKDF2 by default, Argon2id-swappable), per-user JWT claims, account lockout, and a bootstrapped initial admin, plus the matching frontend email+password login."
todos:
  - id: core
    content: "Core: add User entity, UserRole enum, AuthenticatedUserDto"
    status: completed
  - id: hasher
    content: "Application: add IPasswordHasher + Pbkdf2PasswordHasher and register in DI"
    status: completed
  - id: authservice
    content: "Application: rework AuthService to async DB-backed lookup with lockout; update IAuthService, AuthErrors, AuthOptions"
    status: completed
  - id: data
    content: "Data: UserConfiguration, DbSet, IUserRepository+impl, UnitOfWork, DI, AddUsers migration"
    status: completed
  - id: bootstrap
    content: "Api: bootstrap initial admin seed on startup after MigrateAsync"
    status: completed
  - id: api
    content: "Api: per-user JWT claims, LoginRequest email + validator, AuthController async"
    status: completed
  - id: frontend
    content: "Frontend: add email to login type, zod schema, and login page form"
    status: completed
  - id: tests
    content: "Tests: add hasher tests, rewrite AuthServiceTests, update JWT + login validator tests; run dotnet test"
    status: completed
isProject: false
---

# Multi-user admin auth (custom hashing)

Moves auth from one shared config password to per-user accounts in the DB with salted password hashes, per-user JWT claims, and account lockout. Keeps the existing clean-architecture patterns (entity -> config -> DbSet -> repository -> UnitOfWork -> DI -> migration) and the `Result` pattern.

## Design decisions (chosen defaults)
- **Hashing:** PBKDF2-HMAC-SHA256, 600k iterations, 128-bit salt, 256-bit subkey, encoded as a single self-describing string (`algo.iterations.salt.hash`). Zero new NuGet dependencies (BCL `Rfc2898DeriveBytes`). Hidden behind `IPasswordHasher` so Argon2id is a drop-in later (would add `Konscious.Security.Cryptography.Argon2`).
- **Login contract change:** `{ password }` -> `{ email, password }`. This is a breaking API change; frontend updated in the same plan.
- **Bootstrap:** keep `Auth:AdminPassword` + new `Auth:AdminEmail` secrets *only* as a first-run seed. On startup, if the users table is empty, create the initial admin (hashed). After that the DB is the source of truth; the plaintext compare path is deleted.
- **Lockout:** `AccessFailedCount` + `LockoutEndUtc` on the user; lock for a cooldown after N failures. Complements the existing per-IP login rate limiter.
- **Roles:** `UserRole` enum (`Admin`, `Staff`) stored as string; emitted as a JWT role claim. Endpoints stay `[Authorize]` (any authenticated staff) for now; role policies are an optional follow-up.

## Backend

### Core (`src/Tacdent.Core`)
- New `Entities/User.cs : AuditableEntity` - `Id (Guid)`, `Email`, `PasswordHash`, `Role (UserRole)`, `IsActive`, `AccessFailedCount`, `LockoutEndUtc (DateTime?)`.
- New `Entities/UserRole.cs` enum (`Admin`, `Staff`).
- New `DTOs/AuthenticatedUserDto.cs` (`Id`, `Email`, `Role`) - carried out of the Application layer into the token generator.

### Application (`src/Tacdent.Application`)
- New `Services/Interfaces/IPasswordHasher.cs` (`string Hash(string password)`, `bool Verify(string password, string encodedHash)`).
- New `Services/Pbkdf2PasswordHasher.cs` implementing it with BCL crypto + `CryptographicOperations.FixedTimeEquals`.
- Rework [AuthService.cs](src/Tacdent.Application/Services/AuthService.cs): becomes `async`, takes `CancellationToken`, signature `Task<Result<AuthenticatedUserDto>> AuthenticateAsync(string email, string password, CancellationToken)`. Looks up user via `IUnitOfWork.Users.GetByEmailAsync`, checks `IsActive` + lockout, verifies hash, updates failure/lockout counters and `SaveChangesAsync`. Always runs a dummy verify on unknown email to keep timing uniform.
- Update `Services/Interfaces/IAuthService.cs` to the async signature.
- Extend [AuthErrors.cs](src/Tacdent.Application/Errors/AuthErrors.cs): keep `InvalidCredentials`; add `AccountLocked`.
- Extend `Options/AuthOptions.cs`: add `AdminEmail`, `MaxFailedAttempts` (default 5), `LockoutMinutes` (default 15). Keep `AdminPassword` for bootstrap only.
- `DependencyInjection.cs`: register `IPasswordHasher -> Pbkdf2PasswordHasher` (Singleton, stateless).

### Data (`src/Tacdent.Data`)
- New `Configurations/UserConfiguration.cs`: key, `Email` required + **unique index**, `PasswordHash` required, `Role` `HasConversion<string>()`, lengths. No `HasData` (hash needs runtime salt; seeded at startup instead).
- `Context/TacdentDbContext.cs`: add `DbSet<User> Users`.
- New `Repositories/Interfaces/IUserRepository.cs : IRepository<User>` with `Task<User?> GetByEmailAsync(string email, CancellationToken)` (tracked, so counters can be updated).
- New `Repositories/UserRepository.cs : Repository<User>`.
- `Repositories/Interfaces/IUnitOfWork.cs` + `UnitOfWork.cs`: add `IUserRepository Users`.
- `DependencyInjection.cs`: register `IUserRepository`.
- New EF migration `AddUsers` (the model snapshot updates automatically).

### Api (`src/Tacdent.Api`)
- [JwtTokenGenerator.cs](src/Tacdent.Api/Auth/JwtTokenGenerator.cs) + `IJwtTokenGenerator`: `GenerateToken(AuthenticatedUserDto user)` emitting `sub`/`NameIdentifier` = id, `Name`/`email`, `Role` = user role (instead of hardcoded `admin`/`Admin`).
- `ViewModels/LoginRequest.cs`: add `Email`.
- [LoginRequestValidator.cs](src/Tacdent.Api/Validators/LoginRequestValidator.cs): `Email` not-empty + email format; `Password` not-empty.
- [AuthController.cs](src/Tacdent.Api/Controllers/AuthController.cs): `await authService.AuthenticateAsync(request.Email, request.Password, ct)`; on success pass the returned user to the generator. Keep `[AllowAnonymous]` + `[EnableRateLimiting("login")]`.
- [Program.cs](src/Tacdent.Api/Program.cs): after `MigrateAsync()`, add an idempotent **bootstrap admin seed** - if `Users` is empty and `Auth:AdminEmail`/`AdminPassword` are set, hash and insert the initial `Admin`. Keep the existing startup validation of `Jwt:Key` and `Auth:AdminPassword`.

## Frontend (`tacdent-frontend`)
- `src/types/index.ts`: `LoginPayload` gains `email`.
- `src/lib/schemas/login.ts`: add `email` (zod `.email()`).
- `src/app/admin/login/page.tsx`: add an email field (`autoComplete="username"`) above password; pass both to `login()`.
- `src/lib/api.ts` `login()` is unchanged structurally (already posts the payload).

## Tests (`tests/Tacdent.UnitTests`)
- New `Application/Pbkdf2PasswordHasherTests.cs`: hash != plaintext, `Verify` true for correct / false for wrong, two hashes of same password differ (random salt).
- Rewrite `Application/AuthServiceTests.cs`: mock `IUnitOfWork`/`IUserRepository` + real hasher - success returns user; unknown email / wrong password -> `InvalidCredentials`; locked-out user -> `AccountLocked`; failure increments counter.
- Update `Api/JwtTokenGeneratorTests.cs` for the new `GenerateToken(user)` signature and per-user claims.
- Update `Api/LoginRequestValidatorTests.cs` for the required email rule.
- Run `dotnet test` - all green.

## Optional follow-ups (not in this plan, recommended before public launch)
- **Refresh tokens + server-side revocation/logout** (short-lived access token + rotating refresh token table) - enables real logout and shorter access-token lifetime.
- **httpOnly + Secure + SameSite cookie storage** instead of `localStorage` to remove the XSS token-theft vector (needs CSRF handling).
- **Role-based authorization policies** (`[Authorize(Roles = "Admin")]`) once Staff vs Admin capabilities diverge.
- **CSP + Permissions-Policy** response headers, and a JWT signing-key rotation procedure.
- **Audit log** of admin logins / actions.

## Verification
- `dotnet build` + `dotnet test` green.
- Manual: first run seeds the admin; `POST /api/auth/login` with `{ email, password }` returns a token whose claims include the user id/email/role; wrong password N times -> `423/401 AccountLocked`; protected appointment endpoints accept the new token.