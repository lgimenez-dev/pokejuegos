# Run with Docker

`docker-compose.yml` starts **PostgreSQL, the API and the web** with one command. It creates and seeds the database
on its own, so you don't need PostgreSQL or the .NET SDK installed (only for the mobile app).

The MAUI app can't run in a container. Run it as usual (see [Run the app](RUN_THE_APP.md)): it reaches
the API at `http://localhost:5224` exactly as when the API runs from Visual Studio.

| Service | URL on your machine | Notes |
|---|---|---|
| `api` | `http://localhost:5224` | Swagger at `/swagger` |
| `web` | `http://localhost:3000` | Activation and password-reset pages |
| `db` | `localhost:5433` | PostgreSQL 17 (user `postgres`). 5433 so it doesn't clash with a local PostgreSQL |

## 1. Prerequisites

[Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine with the Compose plugin).

## 2. Create a `.env` file

Create `.env` in the repository root (next to `docker-compose.yml`). It's git-ignored:

```env
JWT_SECRET=YOUR_RANDOM_SECRET_AT_LEAST_32_CHARACTERS_LONG
POSTGRES_PASSWORD=YOUR_DB_PASSWORD

# Only for Google sign-in (see Configure the API)
GOOGLE_CLIENT_ID=YOUR_GOOGLE_CLIENT_ID
GOOGLE_CLIENT_SECRET=YOUR_GOOGLE_CLIENT_SECRET
```

| Variable | Required | Default |
|---|---|---|
| `JWT_SECRET` | Yes | — (Compose stops with an error if it's missing) |
| `POSTGRES_PASSWORD` | No | `postgres` |
| `GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET` | Only for Google sign-in | empty |

For Google sign-in, the redirect URI to register is the same as without Docker:
`http://localhost:5224/api/auth/oauth2/google/callback` (see [Configure the API](CONFIGURE_API.md#google-sign-in-optional)).

## 3. Set the SMTP account

The database is seeded from `db/seed.sql`, so put your SMTP account in it **before the first start**, as explained in
[Create the database](CREATE_DATABASE.md). With the placeholders left in, registration fails when it tries to send the
activation email. It can also be changed later through `PUT /api/smtp-config` as the admin.

## 4. Start everything

```bash
docker compose up --build
```

The first start builds both images and runs `db/script.sql` and `db/seed.sql`. The API waits until the database is
ready. Stop with `Ctrl+C`, or run it in the background with `docker compose up --build -d` and stop it with
`docker compose down`.

## Reset the database

The init scripts only run when the data volume is empty. To start again from scratch (for example, after changing
`db/script.sql` or `db/seed.sql`):

```bash
docker compose down -v
docker compose up --build
```

`-v` deletes the database volume, including every user created so far.

## Notes

- The API and the web use ports 5224 and 3000. Stop the ones started from Visual Studio before running Compose, or
  the ports will clash.
- Inside the Compose network, the web reaches the API at `http://api:8080/` and the API reaches the database at
  `db:5432`. The `.env` values are passed to the API as environment variables, as in
  [Configure the API](CONFIGURE_API.md#option-b--environment-variables).
- `appsettings.Development.json` is excluded from the images (`.dockerignore`), so local credentials never end up in
  a container.
