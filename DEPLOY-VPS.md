# TacDent API — VPS deploy (Docker + MSSQL Express)

The API and its database run on the same Ubuntu VPS as the Next.js site. They are not published on the internet. Host nginx, DNS, and TLS stay pointed at the frontend only.

The old IIS deploy (`DEPLOY.md`, `scripts/publish-iis.sh`) is not used for this path.

## Topology

```text
Internet
  → UFW (22 / 80 / 443)
  → host nginx (TLS for tugceaydincignakli.com)
  → 127.0.0.1:3000  TacDent Next.js  (172.30.0.10 on network tacdent)
       → http://api:8080
       → mssql:1433  database tacdent, login tacdent_app
  Host loopback debug only: API 127.0.0.1:8082, MSSQL 127.0.0.1:1433
```

Do not add an nginx `server` for the API. Do not bind `0.0.0.0`. Do not join the `cakmakci` or `himmet` networks.

## Port and network map

| Host bind | Owner |
|-----------|--------|
| `127.0.0.1:3000` | TacDent frontend |
| `127.0.0.1:3001` | Çakmakcı frontend |
| `127.0.0.1:3002` | Himmet frontend |
| `127.0.0.1:3003` | Pablika frontend |
| `127.0.0.1:8080` | Çakmakcı API |
| `127.0.0.1:8081` | Himmet API |
| `127.0.0.1:8082` | TacDent API |
| `127.0.0.1:1433` | TacDent MSSQL |
| `127.0.0.1:5432` | Çakmakcı Postgres |
| `127.0.0.1:5433` | Himmet Postgres |
| Docker network `tacdent` `172.30.0.0/16` | TacDent API + frontend. Frontend address `172.30.0.10` |

Confirm with `ss -H -ltn` and `docker network inspect` before the first deploy. A busy port or an overlapping subnet is a stop.

## What the first boot creates

`docker compose up` starts MSSQL only until the empty database and `tacdent_app` login exist. The API is started after that, on purpose.

On its first start the API runs `Database.MigrateAsync()`, which creates the schema and the migration seed (5 services, 3 testimonials). `AdminSeeder` then inserts `ADMIN_EMAIL` / `ADMIN_PASSWORD` because `Users` is empty. Later starts do neither again: the migration history and the user row are already there. Changing `ADMIN_PASSWORD` in `.env` after the first boot does not change the login password.

`scripts/vps/sync-local-content.sql` then applies the service prices and durations found in the local development database and removes the unused Orthodontics seed row.

## First deploy

Path: `/opt/tacdent/tacdent-backend`. Frontend stays `/opt/tacdent/tacdent-frontend` and is switched only after this API is healthy.

1. Read-only capacity check. Continue only on `x86_64`, with at least 4 GiB available memory, 25 GiB free disk, and no overlap on the ports and subnet above.

   ```bash
   hostname
   uname -m
   free -h
   df -h / /var/lib/docker
   ss -H -ltn
   docker network ls
   ```

2. Clone, then fast-forward `main`. Record `git rev-parse HEAD`.

3. Create `.env` from `.env.vps.example` with `umask 077`. Generate `MSSQL_SA_PASSWORD`, `APP_DB_PASSWORD`, and `JWT_KEY` on the server. Set `ADMIN_EMAIL=admin@tugceaydincignakli.com` and type `ADMIN_PASSWORD` into the file without echoing it. Copy `INTERNAL_API_KEY` from the frontend `.env`. Set `RECAPTCHA_ENABLED` to match whether that frontend `.env` has a site key. `TRUSTED_PROXY_IP=172.30.0.10`.

   Check the file with `ls -l .env` (`-rw-------`) and `docker compose config --quiet`. Do not print `.env`.

4. Start the database only:

   ```bash
   docker compose pull mssql
   docker compose up -d mssql
   ```

   Wait until `docker compose ps` shows `mssql` healthy. `ss` must show `127.0.0.1:1433` and not `0.0.0.0:1433`.

5. Create the empty database and the application login. `APP_DB_PASSWORD` is read from the environment; it is not written into the SQL file.

   ```bash
   set -a; . ./.env; set +a
   docker compose exec -T -e APP_DB_PASSWORD mssql \
     sh -c '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -b' \
     < scripts/vps/create-db-and-app-login.sql
   ```

   The last result row should be `tacdent`, collation `SQL_Latin1_General_CP1_CI_AS`, compatibility `160`.

6. Build and start the API. `logs/` must be writable by uid 1654, the image's non-root user.

   ```bash
   mkdir -p logs && chown 1654:1654 logs
   export GIT_SHA="$(git rev-parse HEAD)"
   docker compose build api
   docker compose up -d --no-deps api
   ```

   Repeat `curl -sS -o /dev/null -w '%{http_code}' http://127.0.0.1:8082/api/services` until it returns `200`.

7. Confirm the boot result before changing content: 4 rows in `__EFMigrationsHistory`, one user (`admin@tugceaydincignakli.com`, `Admin`), 5 services, 3 testimonials, 0 appointments.

8. Apply the local content delta:

   ```bash
   docker compose exec -T mssql \
     sh -c '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -b -W -d tacdent' \
     < scripts/vps/sync-local-content.sql
   ```

   Expect 4 services: Genel Muayene 1000 TRY / 30 min, Diş Beyazlatma 5000 TRY / 120 EUR, Diş İmplantı 20000 TRY / 600 EUR, Acil Bakım 2000 TRY. Ortodonti is gone.

9. Security checks from the host:

   - `GET /api/services` → `200`
   - `GET /scalar` → `404`
   - `POST /api/auth/login` without `X-Internal-Api-Key` → `403`
   - `GET /api/appointments` without a JWT → `401`
   - `ss` shows `1433` and `8082` on `127.0.0.1` only
   - image label `org.opencontainers.image.revision` equals `GIT_SHA`

Do not recreate the frontend in this step. Pointing the site at the new API is the frontend release.

## Rollback before the frontend switches

```bash
docker compose stop api
```

The public site keeps its current behavior, because nginx still serves the existing frontend and that frontend is still aimed at the old API address. `docker compose down -v` deletes the database volume. Do not use `-v`.

## Notes

- Express is the production edition (`MSSQL_PID=Express`): free, 10 GB per database, memory capped at 2048 MB inside a 2560 MB container so it cannot exhaust the VPS.
- `tacdent_app` is `db_owner` in `tacdent` only. The API needs that role because it applies EF migrations on startup. It is not `sa`.
- `sa` is used only for the two SQL scripts. Store the SA password in a password manager; it is the emergency key.
- Automated backups are a separate job. Until the first real appointment is stored, this database can be recreated from the repo and these scripts.
