# Achievements — pending design

Status: **under discussion, nothing implemented.** The language topic it was waiting on is resolved (see
[Dependency on the language decision](#dependency-on-the-language-decision)). This document keeps the context so the
discussion can resume where it stopped. Once decided, the final version moves into `DESIGN.md`, `script.sql`,
`seed.sql` and `query.sql`, and this file is deleted.

## Goal

- Each game has its own list of achievements. Adding a new game tomorrow means adding its achievements as rows
  linked to that game.
- The user unlocks achievements **while playing** (mid-run, not only when the run ends).
- From their profile, the user sees the achievements of each game, unlocked and still locked.

## Proposed tables

### `achievements` — catalog, one row per achievement, linked to a game

```sql
CREATE TABLE IF NOT EXISTS achievements (
    id          uuid         NOT NULL DEFAULT uuid_generate_v4(),
    game_id     uuid         NOT NULL,
    code        varchar(50)  NOT NULL,             -- stable slug, e.g. 'first-guess', 'streak-10'
    name        varchar(100) NOT NULL,             -- internal; the app shows a translated name/description looked up by code
    sort_order  integer      NOT NULL DEFAULT 0,   -- display order in the profile
    created_at  timestamptz  NOT NULL DEFAULT now(),
    updated_at  timestamptz,
    CONSTRAINT "PK_achievements"           PRIMARY KEY (id),
    CONSTRAINT "FK_achievements_game"      FOREIGN KEY (game_id) REFERENCES games (id) ON DELETE CASCADE,
    CONSTRAINT "UQ_achievements_game_code" UNIQUE (game_id, code)
);
```

- `code` is unique **per game**, not globally: two games can each have a `first-play`.
- Same approach as `games`: `name` is internal and the app translates by `code`.

### `user_achievements` — what each user unlocked

One row per unlocked achievement. A missing row means the achievement is locked.

```sql
CREATE TABLE IF NOT EXISTS user_achievements (
    user_id         uuid        NOT NULL,
    achievement_id  uuid        NOT NULL,
    unlocked_at     timestamptz NOT NULL,                -- device time when it was earned (may have been offline)
    created_at      timestamptz NOT NULL DEFAULT now(),  -- when the server received it
    CONSTRAINT "PK_user_achievements"             PRIMARY KEY (user_id, achievement_id),
    CONSTRAINT "FK_user_achievements_user"        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,
    CONSTRAINT "FK_user_achievements_achievement" FOREIGN KEY (achievement_id) REFERENCES achievements (id) ON DELETE CASCADE
);
```

- No `updated_at`: an unlocked achievement never changes.
- The composite primary key (same idea as `user_configs`) makes the sync idempotent.

### Seed — achievements linked to games by `code`, not by UUID

```sql
INSERT INTO achievements (game_id, code, name, sort_order)
SELECT g.id, v.code, v.name, v.sort_order
FROM (VALUES
    ('roulette', 'first-guess', 'First correct guess',   1),
    ('roulette', 'streak-10',   '10 correct in a row',   2),
    ('roulette', 'score-500',   '500 points',            3),
    ('roulette', 'played-25',   'Play 25 games',         4),
    ('pokedex',  'first-fav',   'First favorite',        1),
    ('pokedex',  'team-6',      'Full team (6)',         2)
) AS v(game_code, code, name, sort_order)
JOIN games g ON g.code = v.game_code
ON CONFLICT (game_id, code) DO NOTHING;
```

These achievements are **examples only**; the actual list is still to be decided (open question 6).

## Who decides an achievement is unlocked

"While playing" means mid-run: for example, "10 correct in a row" is only visible to the app, because the server
only receives the final score.

**Proposal (recommended): the app evaluates, the database is the catalog and the record.**

- **App:** each achievement's rule lives in code, keyed by its `code`. The "Achievement unlocked!" notice is
  instant and works offline and without a session, like favorites.
- **Database:** source of truth for the catalog (valid codes, locked achievements to show in the profile) and
  for what the user unlocked, so it survives across devices.
- **New games** already require a new app version (game code, translations), so rules living in the app don't
  add a release. The database only needs the catalog INSERT.
- **Cheating risk:** accepted. Ranking scores are already reported by the client, and achievements are not
  competitive.

**Discarded alternative: the server evaluates rules stored as data** (`condition_type` + `threshold` columns).
New achievements could be added without an app release, but only for conditions the server knows (`points`,
`times_played`); it can't see mid-run events and it would add a rule engine. Not worth it for now.

## Sync between the app and the API

- Unlocks made without a session are stored locally (SQLite) and uploaded on login, like favorites.
- **Union, not last-write-wins.** An achievement never gets locked again, so there are no conflicts like with
  favorites and settings: the app sends `[{gameCode, code, unlockedAt}]`, the server inserts the ones it
  doesn't have and, on a duplicate, keeps the earliest date. It answers with the full unlocked list so the app
  fills in what it was missing (e.g. unlocked on another device).

```sql
-- Sync one unlock (repeated per item, or via unnest of arrays)
INSERT INTO user_achievements (user_id, achievement_id, unlocked_at)
VALUES ($1, $2, $3)
ON CONFLICT (user_id, achievement_id)
DO UPDATE SET unlocked_at = LEAST(user_achievements.unlocked_at, EXCLUDED.unlocked_at);
```

```sql
-- Profile: every achievement grouped by game, with the user's unlock date (null = locked)
SELECT g.code AS game_code, a.code, a.sort_order, ua.unlocked_at
FROM achievements a
JOIN games g ON g.id = a.game_id
LEFT JOIN user_achievements ua ON ua.achievement_id = a.id AND ua.user_id = $1
ORDER BY g.code, a.sort_order;
```

## Draft endpoints

| Endpoint | Purpose |
|---|---|
| `GET /api/achievements` | Full catalog grouped by game, with `unlockedAt` (or null) for the current user. Feeds the profile screen |
| `POST /api/achievements/sync` | Receives `[{gameCode, code, unlockedAt}]`; 400 on unknown codes; upsert; returns the full state |

Backend pieces when implemented: `Achievement` and `UserAchievement` entities, repositories exposed on
`IUnitOfWork`, EF configurations, `Application/Achievements/` service + validator, `AchievementsController`.

## Open questions

1. **Cumulative achievements** (e.g. "Guess 100 Pokémon in total"). Today `user_game_data.metadata` only keeps the
   best run, not totals. Options: (a) keep the counter only in the app, (b) add `total_guessed` to `metadata`,
   (c) leave them out of v1. Leaning towards (a) or (c).
2. **Secret achievements:** an `is_hidden` column that hides the description until unlocked. Only if wanted now
   (YAGNI).
3. **Pokédex achievements:** the design allows them (the Pokédex is a row in `games`). Do we want them?
4. **Global achievements** (not tied to a game, e.g. "Play every game"): they don't fit with
   `game_id NOT NULL`. If they'll ever be wanted, decide now (nullable `game_id` or a reserved "global" game).
5. ~~**Inconsistent game code**~~: resolved, `DESIGN.md` now uses `roulette` like `seed.sql` and the app.
6. **Actual achievement list** for the roulette, based on its mechanics (20 points per correct guess, difficulty
   tier every 5 correct guesses): e.g. "reach tier 5", "streak of X".

## Dependency on the language decision

**Resolved:** translations live in the clients and the API returns codes, never display texts (see "Languages" in
`DESIGN.md`). So `achievements.name` stays internal and in English, the app translates each achievement's name and
description by `code`, and new sync errors are added to `ErrorMessages` as codes (e.g. `UNKNOWN_ACHIEVEMENT`).

What was being weighed:

- `seed.sql` has Spanish data (`games.name = 'Quién es ese Pokémon'`) while the convention is English, and
  `ErrorMessages` is English only. The app doesn't yet let the user choose a language.
- Achievements add more user-facing texts (name + description per achievement). Whatever is decided for games
  and errors applies here:
  - if translations live in the app (current approach for game names), `achievements.name` stays internal and in
    English, and the API never returns display texts;
  - if the API returns translated texts, the catalog would need translation columns or a translations table, and
    the endpoints would need to know the language (`Accept-Language` or `user_configs.language`).
- New `ErrorMessages` for the sync endpoint (e.g. unknown achievement code) follow whatever is decided for error
  messages.
