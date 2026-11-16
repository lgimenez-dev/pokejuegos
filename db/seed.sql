-- Seed: initial admin user (password: 1234 — change it after the first login)
INSERT INTO users (id, email, username, password, role, status, provider)
VALUES (
    uuid_generate_v4(),
    'admin@pokejuegos.dev',
    'admin',
    '$2a$08$vTeFbl3cIogJx5..USDYkOkSL6c8SLKHQORjYPgCwuOXWEKUAIxPW',
    'ADMIN',
    'ACTIVE',
    'LOCAL'
)
ON CONFLICT DO NOTHING;


-- Seed: initial game catalog
INSERT INTO games (code, name, has_ranking) VALUES
    ('pokedex', 'Pokédex', false),
    ('roulette', 'Quién es ese Pokémon', true)
ON CONFLICT (code) DO NOTHING;


-- Seed: SMTP account used to send the activation and password recovery emails.
-- Replace the placeholders before running this script (for Gmail, use an App Password:
-- https://myaccount.google.com/apppasswords). It can also be changed later through PUT /api/smtp-config.
INSERT INTO smtp_configs (host, port, email, password, sender_name, use_tls)
VALUES ('smtp.gmail.com', 587, 'YOUR_EMAIL@gmail.com', 'YOUR_APP_PASSWORD', 'PokeJuegos', true);
