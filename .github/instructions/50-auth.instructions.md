---
applyTo: "src/Tacdent.Api/Auth/**/*.cs,src/Tacdent.Api/Controllers/AuthController.cs,src/Tacdent.Application/Services/AuthService.cs,src/Tacdent.Application/Services/Interfaces/IAuthService.cs,src/Tacdent.Application/Options/**/*.cs,src/Tacdent.Application/Errors/AuthErrors.cs"
---

# Auth (JWT bearer + Users table)

Staff sign in with email and password. `AuthService.AuthenticateAsync` checks the `Users` row
(PBKDF2 hash, active flag, lockout) and returns `Result<AuthenticatedUserDto>`. Roles are `Admin`
and `Staff`. The API mints a JWT from that user; the Next.js BFF stores it in an httpOnly cookie.

`AdminSeeder` creates `Auth:AdminEmail` / `Auth:AdminPassword` only when `Users` is empty. After
that the database is the source of truth. Changing the env password does not change the login.

## Layering
- **Application** owns the credential check and password hashing. `AuthErrors.InvalidCredentials`
  and `AuthErrors.AccountLocked` are `Error.Unauthorized(...)`.
- **Api** owns token minting: `IJwtTokenGenerator.GenerateToken(user)` returns `(token, expiresAt)`,
  registered **Singleton**. `LoginResponse` is `(token, expiresAt, role)`.
- `AuthController` (`POST /api/auth/login`, `[AllowAnonymous]`) validates reCAPTCHA first, then
  `AuthenticateAsync`. Failure becomes `ToProblemResult()` (401).

## Program.cs wiring (don't reorder)
- Bind `JwtOptions` via `Configure<JwtOptions>(...GetSection(JwtOptions.SectionName))`.
- `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` with
  `TokenValidationParameters` validating issuer/audience/lifetime/signing key from `JwtOptions`.
- `AddAuthorization()`, and call **`app.UseAuthentication()` before `app.UseAuthorization()`**.

## Protecting endpoints
- Management controllers are `[Authorize]` at the class level. Public actions are
  `[AllowAnonymous]` (`auth/login`, booking `Create`, public service and testimonial lists).
- Admin-only actions add `[Authorize(Roles = Roles.Admin)]` (user management, deletes, assignment).

## Security
- `Jwt:Key` is at least 32 characters and is never committed. Same for `Auth:AdminPassword`.
- Login is rate-limited (`login` policy). Failed attempts lock the user (`MaxFailedAttempts`,
  `LockoutMinutes`).
- `POST /api/auth/login` and `POST /api/appointments` require `X-Internal-Api-Key` when
  `InternalApi:Key` is set. The BFF sends it. Do not accept that header from the public internet.
