# Database design — decisions

A record of what was decided for the PokeJuegos PostgreSQL database and how each part is handled.
The scripts are `script.sql` (tables), `seed.sql` (initial data) and `query.sql` (reference queries for the
backend). The database hasn't been created yet.

## New or modified tables

### `games` — catalog

| Column | Type | Notes |
|---|---|---|
| `code` | `varchar(50)` | Stable identifier: `pokedex`, `quien-es-ese-pokemon`, `battle-survivor` |
| `name` | `varchar(100)` | Internal name. The app shows the translated name based on `code` |
| `has_ranking` | `boolean` | `true` for games with a score; `false` for the Pokédex |

### `user_game_data` — a user's state in a game

One row per user and game (`UNIQUE (user_id, game_id)`), updated in place. **It is not a history of
plays.**

| Column | Type | Notes |
|---|---|---|
| `points` | `integer`, default 0 | Best score. Always 0 in the Pokédex |
| `metadata` | `jsonb` | Always an object; see [`metadata` format](#metadata-format) |
| `times_played` | `integer`, default 0 | Goes up by 1 on every play, whether the score improves or not |
| `best_at` | `timestamptz`, nullable | When the best score was reached. Breaks ties in the ranking. `NULL` in the Pokédex |
| `updated_at` | `timestamptz` | Last change to the row: last play, or last change to the favorites |

Ranking index: `(game_id, points DESC, best_at)`.

### `user_configs` — user preferences

1:1 relationship with `users` (`user_id` is the primary key). The row is created the first time the user saves
something; if it doesn't exist, the defaults are used.

| Column | Type | Notes |
|---|---|---|
| `language` | `varchar(2)`, nullable | `'en'` or `'es'`. `NULL` = use the device language |
| `sound` | `boolean`, default `true` | |
| `updated_at` | `timestamptz` | Time of the last change; decides which version wins when syncing |

It is a separate table so more preferences can be added in the future without mixing them with the personal data
in `persons`.

## Rules when recording a play

Only for games with `has_ranking = true`. On every play:

- `times_played` goes up by 1 **always**.
- `points`, `metadata` and `best_at` are replaced **only if the new score beats the stored one**. A worse play
  never overwrites the best one.
- `updated_at` is always updated (it works as "last time played").

### Why both `best_at` and `updated_at` are needed

The ranking breaks ties by who reached the score first. If it used `updated_at`, which changes on every play,
playing without improving would push a player down the ranking:

- A scores 500 points on Monday and B scores 500 on Tuesday: A goes first.
- On Wednesday A plays again and scores 300. With `updated_at`, A would fall behind B even though their best score
  didn't change.

`best_at` only changes when the score improves, so the tiebreak stays stable. Each date has a single
meaning.

## `has_ranking`

Records in the catalog which games have a score, so the code doesn't depend on comparing against `"pokedex"`.
The API uses it to:

1. **Choose the upsert:** a play (add to `times_played`, "only if it beats it" rule) or the Pokédex (replace the
   favorites).
2. **Reject meaningless requests:** asking for the ranking or submitting a score for the Pokédex returns 400.
3. **Tell the app** which games show a ranking and top scores.

## `metadata` format

Always a **JSON object**, never a bare array, so fields can be added without changing the format:

| Game | Format | Content |
|---|---|---|
| `quien-es-ese-pokemon` | `{"guessed": 42}` | Pokémon guessed in the best play |
| `pokedex` | `{"favorites": [1, 4, 25]}` | Ids of the favorite Pokémon, without duplicates and sorted |

The database doesn't validate the content of `metadata`. The **API** validates the shape of each game's object and
that favorite ids are between 1 and 386 (Generations I–III).

## Languages

- The app's interface supports English and Spanish.
- `user_configs.language` set to `NULL` means "follow the device language", so a new user isn't forced into a
  language.
- The backend also uses the saved language for emails (activation, password recovery).
- Game names are translated in the app based on `games.code`; `games.name` is internal only.

## Syncing between the app and the API

The app works offline and without a session; the server holds a copy that is kept in sync.

- **Local storage:** favorites live in the SQLite table `favorite_pokemon`, one row per Pokémon.
  When syncing, the app gathers the ids into an array and sends it to the API as `int[]`.
- **Sync state:** for both favorites and settings, the app stores the time of the last change and a
  "pending upload" flag (in a `sync_state` table or in `Preferences`).
- **When it's sent:** a few seconds after the last change, and also on login, when the app opens and when the user
  returns to it, if there is something pending and a connection.
- **Who wins:** the most recent change according to `updated_at`. The app sends everything along with the time of
  its last change. If the server's copy is older, the new data is saved; if it's newer, nothing is written and the
  app downloads the server's version.
- **First login:** if the phone has favorites saved without a session and the server has some too, the two lists
  are merged instead of one overwriting the other.
- **Accepted limitations:** if favorites are changed offline on two devices at the same time, the most recent
  change is kept and the other one is lost. The time is set by the phone, so a misconfigured clock can tip the
  result.

Favorites and settings are synced separately, each with its own `updated_at`.

## Username for OAuth users

- Users who sign up with OAuth fill in a form after authenticating to choose their username.
- In the database `users.username` is optional, because the user exists between authentication and that
  form.
- The API doesn't allow saving scores until the user has a `username`, so the ranking never shows someone
  without a name.

## Next steps

1. Create the database with `script.sql` and `seed.sql`, and test the queries in `query.sql` with sample data.
2. Backend: entities, repositories and services for `user_game_data` and `user_configs` (with EF Core + Npgsql,
   `metadata` as `jsonb`).
3. Endpoints: record a play, ranking and position, favorites (read and sync), settings (read and
   sync).
4. Username form after logging in with OAuth.
5. App: sync of favorites and settings, and texts in English and Spanish.
