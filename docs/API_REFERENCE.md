# API reference

Base URL: `http://localhost:5224`. Interactive docs: `http://localhost:5224/swagger` (Development only).

Authenticated endpoints accept the JWT from the `access_token` HttpOnly cookie or from an
`Authorization: Bearer <token>` header (the mobile app uses the header). For the step-by-step behavior of each auth
endpoint, see [Authentication flow](FLOW_AUTH_REGISTER.md).

## Auth — `/api/auth`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/register` | Public | Creates a pending account and emails an activation link |
| POST | `/activate` | Public | Activates a pending account with the one-time token from the email |
| POST | `/login` | Public | Logs in with email or username. Returns the JWT and sets an `access_token` HttpOnly cookie. 5 req/min per IP |
| POST | `/logout` | JWT | Revokes the current session |
| GET | `/validate-token` | JWT | Confirms the session is still active in the database |
| GET | `/me` | JWT | Identity claims from the JWT |
| POST | `/recover-password` | Public | Emails a reset link. Always answers 200, so it can't be used to check which accounts exist. 3 req/min per IP |
| POST | `/change-password` | Public | Sets a new password with the one-time token from the email |
| PUT | `/profile` | JWT | Sets username and name (Google users are created without a username) |
| GET | `/oauth2/google` | Public | Starts Google sign-in. Native clients pass a validated `returnUrl` |

## Sessions — `/api/sessions`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/user/{userId}` | JWT | Active sessions of a user |
| POST | `/user/{userId}/revoke` | JWT | Revokes all active sessions of a user |

## SMTP config — `/api/smtp-config`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/` | Admin | Current SMTP configuration |
| POST | `/` | Admin | Creates it (single record) |
| PUT | `/` | Admin | Replaces it |

## Errors

| Status | Body | When |
|---|---|---|
| 400 | `ValidationErrorResponse` | A request field fails validation (field-level errors) |
| 400 | `BusinessErrorResponse` | A business rule fails (e.g. email already registered, expired token) |
| 401 | `BusinessErrorResponse` | Login fails (invalid credentials, pending or inactive account), or the session to log out / validate isn't active |
| 401 | — | Missing or invalid JWT on an authenticated endpoint |
| 403 | — | Authenticated, but not an admin (`/api/smtp-config`) |
| 404 | `BusinessErrorResponse` | SMTP config or user sessions not found |
| 429 | — | Rate limit exceeded (`/login`, `/recover-password`) |
| 500 | — | Unexpected error (handled by `ExceptionHandlingMiddleware`) |

`BusinessErrorResponse` is `{ "message": "..." }`.
