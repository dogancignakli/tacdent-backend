# TacDent Backend — IIS / FTP deployment

Deploy the API to a Windows/IIS shared host with MSSQL using FileZilla.

## Prerequisites

- Hosting panel: MSSQL database created, subdomain bound to the site folder
- FileZilla FTP credentials
- Frontend production URL (for `Cors:Origins`)
- Strong secrets: admin password, JWT key (32+ chars), `InternalApi:Key`

## 1. Configure production secrets

```bash
cp src/Tacdent.Api/appsettings.Production.example.json src/Tacdent.Api/appsettings.Production.json
```

Edit `src/Tacdent.Api/appsettings.Production.json` with real values from your hosting panel:

| Setting | Source |
|---------|--------|
| `ConnectionStrings:DefaultConnection` | MSSQL server, database, user, password |
| `Cors:Origins` | `https://your-frontend-domain.com` |
| `Auth:AdminEmail` / `Auth:AdminPassword` | First-run bootstrap admin (change after login) |
| `Jwt:Key` | Random string, at least 32 characters |
| `Recaptcha:SecretKey` | Google reCAPTCHA v3 secret (or set `Enabled: false` temporarily) |
| `InternalApi:Key` | Same value as frontend `INTERNAL_API_KEY` |

This file is gitignored — never commit it.

## 2. Build publish output

```bash
chmod +x scripts/publish-iis.sh
./scripts/publish-iis.sh
```

Output lands in `./publish/` (also gitignored). The script:

- Publishes **self-contained** `win-x64` (no .NET 10 runtime required on host)
- Sets `ASPNETCORE_ENVIRONMENT=Production` in `web.config`
- Enables stdout logging to `publish/logs/`

## 3. Upload via FileZilla

1. Connect with FTP credentials from your hosting panel.
2. Open the site root folder (`httpdocs`, `wwwroot`, or `site/wwwroot`).
3. For **first deploy**: upload everything inside `./publish/`.
4. For **redeploy**: upload `scripts/app_offline.htm` first, replace files, then delete `app_offline.htm`.
5. Confirm `appsettings.Production.json` is present in the site root on the server.

## 4. Database permissions

The API runs EF migrations on startup (`Program.cs`). The MSSQL login must be able to create/alter tables in your database. On typical shared hosting, the database user you create in the panel already has full rights on that database.

On first successful start, `AdminSeeder` creates the initial admin from `Auth:AdminEmail` / `Auth:AdminPassword` if the users table is empty.

## 5. SSL on subdomain

In the hosting panel:

1. Enable SSL / Let's Encrypt for your API subdomain (e.g. `api.example.com`).
2. Force HTTPS if the panel offers it.

The API uses `UseHttpsRedirection()` and HSTS outside Development.

If KVKK consent records show the wrong client IP, add your host's reverse-proxy IP to `ForwardedHeaders:KnownProxies` in `appsettings.Production.json` and redeploy.

## 6. Verify production

Replace `https://api.example.com` with your subdomain.

```bash
chmod +x scripts/verify-production.sh
./scripts/verify-production.sh https://api.example.com

# Optional login check:
TACDENT_ADMIN_EMAIL=admin@example.com TACDENT_ADMIN_PASSWORD='<password>' \
  ./scripts/verify-production.sh https://api.example.com
```

Manual checks:

- `GET /api/services` → `200` with JSON array
- `POST /api/auth/login` → `200` with `{ token, expiresAt, role }` (when reCAPTCHA disabled or token provided)
- `GET /scalar` → **404** (docs disabled in Production — correct)

If the site returns 500, check `logs/stdout_*.log` on the server via FTP.

## 7. Wire the frontend

In your frontend host (e.g. Vercel), copy [tacdent-frontend/.env.production.example](../tacdent-frontend/.env.production.example) and set:

```env
NEXT_PUBLIC_API_URL=https://api.example.com
API_URL=https://api.example.com
NEXT_PUBLIC_SITE_URL=https://www.example.com
NEXT_PUBLIC_RECAPTCHA_SITE_KEY=<your-site-key>
INTERNAL_API_KEY=<same as InternalApi:Key on API>
```

Backend `Cors:Origins` must include `https://www.example.com` (your real frontend URL).

## Troubleshooting

| Symptom | Likely cause |
|---------|----------------|
| HTTP 500.31 / 500.32 | Missing runtime or in-process host — use self-contained publish and `hostingModel="outofprocess"` in `web.config` |
| App won't start | Placeholder secrets in `appsettings.Production.json` |
| Login/booking 403 from API | `InternalApi:Key` mismatch with frontend `INTERNAL_API_KEY` |
| CORS errors | `Cors:Origins` missing frontend URL |
| DB connection failed | Wrong connection string; check server name and `Encrypt=True` |
