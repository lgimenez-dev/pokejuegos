
-- Register a play (only games with has_ranking = true; the API checks it).
-- times_played always goes up. points, metadata and best_at are only replaced when the new score beats the
-- stored one: a worse run never overwrites the best. Every CASE reads the row as it was before this update.
INSERT INTO user_game_data (user_id, game_id, points, metadata, times_played, best_at)
VALUES ($1, $2, $3, $4, 1, now())  -- $4: e.g. '{"guessed": 42}'
ON CONFLICT (user_id, game_id)
DO UPDATE SET
    times_played = user_game_data.times_played + 1,
    points       = CASE WHEN EXCLUDED.points > user_game_data.points THEN EXCLUDED.points   ELSE user_game_data.points   END,
    metadata     = CASE WHEN EXCLUDED.points > user_game_data.points THEN EXCLUDED.metadata ELSE user_game_data.metadata END,
    best_at      = CASE WHEN EXCLUDED.points > user_game_data.points THEN now()             ELSE user_game_data.best_at  END,
    updated_at   = now();


-- Ranking: top 10 of a game. On a tie, whoever reached that score first goes ahead.
SELECT u.username, ugd.points, ugd.metadata
FROM user_game_data ugd
JOIN users u ON u.id = ugd.user_id
WHERE ugd.game_id = $1
ORDER BY ugd.points DESC, ugd.best_at
LIMIT 10;


-- Player position: the user's own place in a game's ranking, even outside the top 10.
-- Same ordering as the ranking, so the position matches the list. No row = the user hasn't played this game.
SELECT ranked.position, ranked.points, ranked.metadata
FROM (
    SELECT ugd.user_id,
           ugd.points,
           ugd.metadata,
           ROW_NUMBER() OVER (ORDER BY ugd.points DESC, ugd.best_at) AS position
    FROM user_game_data ugd
    WHERE ugd.game_id = $1
) ranked
WHERE ranked.user_id = $2;


-- Pokedex favorites: read. No row = no favorites yet.
SELECT ugd.metadata -> 'favorites' AS favorites, ugd.updated_at
FROM user_game_data ugd
JOIN games g ON g.id = ugd.game_id
WHERE ugd.user_id = $1
  AND g.code = 'pokedex';


-- Pokedex favorites: sync from the app (the whole list, last write wins).
-- $3 is the list as integer[] (duplicates removed, sorted); $4 is the time of the last local change.
-- If the server copy is newer, nothing is written and no row comes back: the app then reads the favorites
-- and replaces its local copy.
INSERT INTO user_game_data (user_id, game_id, metadata, updated_at)
VALUES (
    $1,
    $2,  -- id of the 'pokedex' game
    jsonb_build_object('favorites', to_jsonb(ARRAY(SELECT DISTINCT unnest($3::integer[]) ORDER BY 1))),
    $4
)
ON CONFLICT (user_id, game_id)
DO UPDATE SET
    metadata   = EXCLUDED.metadata,
    updated_at = EXCLUDED.updated_at
WHERE user_game_data.updated_at < EXCLUDED.updated_at
RETURNING metadata -> 'favorites' AS favorites, updated_at;


-- User config: read. No row = defaults (language null = device language, sound on).
SELECT language, sound, updated_at
FROM user_configs
WHERE user_id = $1;


-- User config: sync from the app (the whole config, last write wins).
-- $4 is the time of the last local change. If the server copy is newer, nothing is written and no row
-- comes back: the app then reads the config and replaces its local copy.
INSERT INTO user_configs (user_id, language, sound, updated_at)
VALUES ($1, $2, $3, $4)
ON CONFLICT (user_id)
DO UPDATE SET
    language   = EXCLUDED.language,
    sound      = EXCLUDED.sound,
    updated_at = EXCLUDED.updated_at
WHERE user_configs.updated_at < EXCLUDED.updated_at
RETURNING language, sound, updated_at;
