CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Users: authentication identity
CREATE TABLE IF NOT EXISTS users (
    id               uuid         NOT NULL DEFAULT uuid_generate_v4(),
    email            varchar(256) NOT NULL,
    username         varchar(32),                   -- optional alternative login identifier
    password         text,                          -- null for OAuth2 users
    role             varchar(20)  NOT NULL DEFAULT 'MEMBER',
    status           varchar(20)  NOT NULL DEFAULT 'ACTIVE',
    refresh_token    varchar(512),                  -- activation / recovery token
    expiration_date  timestamptz,
    provider         varchar(20)  NOT NULL DEFAULT 'LOCAL',
    provider_id      varchar(255),                  -- OAuth2 external user ID
    created_at       timestamptz  NOT NULL DEFAULT now(),
    updated_at       timestamptz,
    CONSTRAINT "PK_users" PRIMARY KEY (id)
);
CREATE UNIQUE INDEX "UQ_users_email"         ON users (email);
CREATE UNIQUE INDEX "UQ_users_username"      ON users (username) WHERE username IS NOT NULL;
CREATE INDEX "IX_users_refresh_token"        ON users (refresh_token) WHERE refresh_token IS NOT NULL;
CREATE INDEX "IDX_users_provider_id"         ON users (provider, provider_id) WHERE provider_id IS NOT NULL;

-- Persons: user profile data (1:1 with users)
CREATE TABLE IF NOT EXISTS persons (
    id           uuid         NOT NULL DEFAULT uuid_generate_v4(),
    user_id      uuid         NOT NULL,
    name         varchar(200) NOT NULL,
    national_id  varchar(20),                       -- nullable: OAuth2 users complete this later
    phone        varchar(20),
    birth_date   date,
    created_at   timestamptz  NOT NULL DEFAULT now(),
    updated_at   timestamptz,
    CONSTRAINT "PK_persons"         PRIMARY KEY (id),
    CONSTRAINT "FK_persons_user"    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,
    CONSTRAINT "UQ_persons_user_id" UNIQUE (user_id)
);
CREATE UNIQUE INDEX "UQ_persons_national_id" ON persons (national_id) WHERE national_id IS NOT NULL;

-- User sessions: active JWT sessions
CREATE TABLE IF NOT EXISTS user_sessions (
    id               uuid         NOT NULL DEFAULT uuid_generate_v4(),
    user_id          uuid         NOT NULL,
    token_hash       varchar(255) NOT NULL,
    user_agent       varchar(512),
    ip               varchar(64),
    expiration_date  timestamptz  NOT NULL,
    revoked_at       timestamptz,
    created_at       timestamptz  NOT NULL DEFAULT now(),
    updated_at       timestamptz,
    CONSTRAINT "PK_user_sessions"      PRIMARY KEY (id),
    CONSTRAINT "FK_user_sessions_user" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);
CREATE INDEX "IDX_user_sessions_user_id"    ON user_sessions (user_id);
CREATE INDEX "IDX_user_sessions_token_hash" ON user_sessions (token_hash);
CREATE INDEX "IDX_user_sessions_created_at" ON user_sessions (created_at);

-- SMTP config: email sending configuration (single row)
CREATE TABLE IF NOT EXISTS smtp_configs (
    id           uuid         NOT NULL DEFAULT uuid_generate_v4(),
    host         varchar(256) NOT NULL,
    port         integer      NOT NULL DEFAULT 587,
    email        varchar(256) NOT NULL,
    password     varchar(256) NOT NULL,
    sender_name  varchar(200) NOT NULL,
    use_tls      boolean      NOT NULL DEFAULT true,
    created_at   timestamptz  NOT NULL DEFAULT now(),
    updated_at   timestamptz,
    CONSTRAINT "PK_smtp_configs" PRIMARY KEY (id)
);

-- Games: catalog of games in the hub (Pokedex + games), one row per game
CREATE TABLE IF NOT EXISTS games (
    id           uuid         NOT NULL DEFAULT uuid_generate_v4(),
    code         varchar(50)  NOT NULL,             -- stable slug, e.g. 'pokedex', 'quien-es-ese-pokemon', 'battle-survivor'
    name         varchar(100) NOT NULL,             -- internal name; the app shows a translated one, looked up by code
    has_ranking  boolean      NOT NULL DEFAULT true, -- false for sections without score (Pokedex): no ranking, no plays
    created_at   timestamptz  NOT NULL DEFAULT now(),
    updated_at   timestamptz,
    CONSTRAINT "PK_games"    PRIMARY KEY (id)
);
CREATE UNIQUE INDEX "UQ_games_code" ON games (code);

-- User game data: a user's state in a game (one row per user+game, updated in place; not a log of plays).
-- Ranked games keep the best score (only overwritten when beaten) and count plays; the Pokedex keeps the favorites.
CREATE TABLE IF NOT EXISTS user_game_data (
    id            uuid         NOT NULL DEFAULT uuid_generate_v4(),
    user_id       uuid         NOT NULL,
    game_id       uuid         NOT NULL,
    points        integer      NOT NULL DEFAULT 0,     -- best score; always 0 in the Pokedex
    metadata      jsonb,                               -- always an object: {"guessed": 42} (best run), {"favorites": [1, 4, 25]} (Pokedex)
    times_played  integer      NOT NULL DEFAULT 0,     -- +1 on every play, beaten or not
    best_at       timestamptz,                         -- when the best score was reached (ranking tiebreak); null in the Pokedex
    created_at    timestamptz  NOT NULL DEFAULT now(),
    updated_at    timestamptz  NOT NULL DEFAULT now(), -- last change of the row: last play, or last favorites change
    CONSTRAINT "PK_user_game_data"              PRIMARY KEY (id),
    CONSTRAINT "FK_user_game_data_user"         FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,
    CONSTRAINT "FK_user_game_data_game"         FOREIGN KEY (game_id) REFERENCES games (id) ON DELETE CASCADE,
    CONSTRAINT "UQ_user_game_data_user_game"    UNIQUE (user_id, game_id),
    CONSTRAINT "CK_user_game_data_points"       CHECK (points >= 0),
    CONSTRAINT "CK_user_game_data_times_played" CHECK (times_played >= 0)
);
-- Leaderboard: filter by game_id, sort by points desc and, on a tie, whoever reached it first
CREATE INDEX "IDX_user_game_data_game_points" ON user_game_data (game_id, points DESC, best_at);

-- User configs: per-user preferences (1:1 with users, created lazily: a missing row means defaults)
CREATE TABLE IF NOT EXISTS user_configs (
    user_id     uuid         NOT NULL,
    language    varchar(2),                          -- 'en' | 'es'; null = follow the device language
    sound       boolean      NOT NULL DEFAULT true,
    created_at  timestamptz  NOT NULL DEFAULT now(),
    updated_at  timestamptz  NOT NULL DEFAULT now(), -- last write wins when syncing from the app
    CONSTRAINT "PK_user_configs"          PRIMARY KEY (user_id),
    CONSTRAINT "FK_user_configs_user"     FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,
    CONSTRAINT "CK_user_configs_language" CHECK (language IN ('en', 'es'))
);
