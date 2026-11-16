# Create the database

PokeJuegos uses PostgreSQL 14 or newer. There are no EF Core migrations: the schema and the initial data are plain
SQL scripts in [`db/`](../db).

| Script | What it does |
|---|---|
| `db/script.sql` | Creates the tables, indexes and constraints |
| `db/seed.sql` | Inserts the admin user, the game catalog and the SMTP account |
| `db/query.sql` | Reference queries for the backend (record a play, ranking, player position). Not meant to be run |

## Steps

From the repository root:

```bash
psql -U postgres -c "CREATE DATABASE pokejuegos;"
psql -U postgres -d pokejuegos -f db/script.sql
```

Before running the seed, open `db/seed.sql` and replace `YOUR_EMAIL@gmail.com` / `YOUR_APP_PASSWORD` with your SMTP
account. For Gmail, use an [App Password](https://myaccount.google.com/apppasswords), not your account password.
Then:

```bash
psql -U postgres -d pokejuegos -f db/seed.sql
```

> On Windows, `psql` lives in `C:\Program Files\PostgreSQL\<version>\bin` and isn't in the `PATH` by default.
> Running the scripts from pgAdmin's Query Tool works too.

## What the seed creates

- An admin user: `admin@pokejuegos.dev` / password `1234`. Change it after the first login.
- The game catalog: `pokedex` and `roulette`.
- The SMTP account used to send the activation and password-reset emails. It can also be changed later through
  `PUT /api/smtp-config` while logged in as the admin (see the [API reference](API_REFERENCE.md#smtp-config--apismtp-config)).

Without an SMTP account, registration still works, but no activation email is sent, so new local accounts stay
pending.

## Design

Table design and the reasoning behind it: [`db/DESIGN.md`](../db/DESIGN.md).
