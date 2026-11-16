# Authentication Flow

Full walkthrough of every path into and out of an authenticated session in PokeJuegos: local
registration + activation, local login (email or username), Google OAuth2, password recovery,
logout, and session validation. Written in English to match the rest of the project's
documentation and naming conventions.

---

## 1. Local registration — `POST /api/auth/register`

**Request** (`RegisterRequest`):

```json
{
  "email": "lucas@example.com",
  "password": "Passw0rd!",
  "name": "Lucas Gimenez",
  "username": "lucasg",        // optional
  "nationalId": "12345678",    // optional
  "phone": "+54 11 5555-5555", // optional
  "birthDate": "1995-04-12"    // optional
}
```

`Email`, `Password`, and `Name` are required. `Username`, if provided, must be 3-32 characters,
matching `^[A-Za-z0-9._-]{3,32}$`, and unique.

**Flow** (`AuthController.Register` → `AuthService.RegisterAsync`):

1. `RegisterValidator` checks format rules (password strength, email format, username format
   when present) — a failure returns `400` with a field-level `ValidationErrorResponse`.
2. `AuthService` checks that `Email` isn't already registered → `EmailAlreadyRegistered` on
   conflict.
3. If `Username` was provided, checks it isn't already taken → `UsernameAlreadyRegistered` on
   conflict.
4. Creates a `User` with `Status = Pending`, `Provider = Local`, `PasswordHash` set via BCrypt,
   and `Username` set if provided.
5. Creates the linked `Person` (name, national ID, phone, birth date).
6. Generates a random activation token (`Guid.NewGuid("N")`), stores it hashed on
   `User.RefreshToken` with a 24-hour expiration (`UpdateToken`).
7. If an `SmtpConfig` row exists, sends an activation email with a link to
   `{FrontendUrl}/activate?token={activationToken}` (served by `PokeJuegos.Web`).
8. Returns `200 OK` (no body). The account is **not usable yet** — it stays `Pending` until
   activation.

```
POST /register
  → validate
  → email unique? username unique (if given)?
  → create User(Pending) + Person
  → generate activation token (24h)
  → send activation email (if SMTP configured)
  → 200 OK
```

## 2. Account activation — `POST /api/auth/activate`

The emailed link opens `PokeJuegos.Web`'s `/activate` page, which posts the token to the API
when the user presses "Activate my account" (not on page load, so mail scanners that prefetch
links don't consume the token).

**Request** (`ActivateAccountRequest`): `{ "token": "<activation token>" }`

**Flow** (`AuthService.ActivateAsync`):

1. Looks up the `User` by `RefreshToken == token`.
2. Rejects if not found, `ExpirationDate` has passed, or the account isn't `Pending` (a recovery
   token lives in the same column and must not activate anything) → `InvalidOrExpiredToken`.
3. Calls `User.Activate()` — flips `Status` to `Active`, keeping the password chosen at
   registration.
4. Clears the token (`RevokeToken`).
5. Returns `200 OK`. The account can now log in.

## 3. Local login — `POST /api/auth/login`

**Request** (`LoginRequest`) — exactly one of `Email`/`Username` must be provided:

```json
{ "email": "lucas@example.com", "username": null, "password": "Passw0rd!" }
```
or
```json
{ "email": null, "username": "lucasg", "password": "Passw0rd!" }
```

Rate-limited to 5 requests/minute per IP (`[EnableRateLimiting("login")]`).

**Flow** (`AuthService.LoginAsync`):

1. `LoginValidator` enforces the XOR rule (exactly one identifier), format of whichever was
   given, and that `Password` is non-empty.
2. Resolves the `User` via `GetByEmailAsync` or `GetByUsernameAsync` depending on which field
   arrived → `InvalidCredentials` if not found.
3. Rejects `Pending` (`AccountPending`) and `Inactive` (`AccountInactive`) accounts.
4. Verifies the password against `PasswordHash` (BCrypt) → `InvalidCredentials` on mismatch.
5. Revokes all of the user's previously active `UserSession` rows (single-session policy).
6. Fetches the linked `Person` to get the display name.
7. Generates a JWT (`sub`, `name`, `email`, `role` claims) and hashes it (SHA-256) for storage.
8. Creates a new `UserSession` row with the token hash, expiration, and (currently) no IP/user
   agent recorded.
9. If the user had a stale activation/recovery token sitting around, clears it.
10. Returns `LoginResponse { Token, Email, Role }`.

**Controller** sets the JWT as an `HttpOnly`, `Secure`, `SameSite=None` cookie named
`access_token` with the same expiration as the token, then returns the body too.

```
POST /login { email XOR username, password }
  → validate (XOR + format)
  → resolve user by email or username
  → check status (Pending/Inactive rejected)
  → verify password
  → revoke previous sessions
  → issue JWT (sub, name, email, role) + persist session (hash)
  → set access_token cookie
  → 200 OK { token, email, role }
```

## 4. Password recovery — `POST /api/auth/recover-password` → `POST /api/auth/change-password`

**Request** (`RecoverPasswordRequest`): `{ "email": "lucas@example.com" }`. Rate limited to 3 requests
per minute per IP (`recover-password` policy), since each one can send an email.

**Flow** (`AuthService.RecoverPasswordAsync`):

1. Looks up the user by email. If not found, or the account is `Pending`, **still returns
   `200 OK`** — this endpoint always succeeds outwardly to prevent user enumeration.
2. Otherwise: generates a recovery token, stores it on `User.RefreshToken` with a
   30-minute expiration, and emails a link to `{FrontendUrl}/reset-password?token={recoveryToken}`
   if SMTP is configured.
3. OAuth2 accounts (Google) have no password: they get the "Create a password" email
   (`EmailHelper.PasswordCreation`) instead, with the same link. Once set, they can also log in with
   email + password. Until then, `/login` answers `InvalidCredentials` for them.
4. `PokeJuegos.Web`'s `/reset-password` page asks for the new password and calls
   `POST /change-password` (`ChangePasswordRequest { token, password }`): looks up the user by
   token, rejects if missing/expired (`InvalidOrExpiredToken`), hashes the new password via
   `User.ChangePassword(hash)`, clears the token and returns `200 OK`.

## 5. Google OAuth2 — `GET /api/auth/oauth2/google`

This is a **single flow that covers both registration and login** for Google accounts — it does
not go through `/register` or `/login` at all.

```
GET /oauth2/google
  → redirect to Google consent screen
  → user authenticates with Google
  → Google redirects to /oauth2/google/callback with claims
  → OAuth2Service.HandleGoogleCallbackAsync(email, name, providerId)
  → set access_token cookie
  → redirect to FrontendUrl (or FrontendUrl?oauth_error=... on failure)
```

**Claims extraction** (`GoogleAuthExtensions.cs`): `ClaimTypes.NameIdentifier` → `providerId`
(Google's immutable `sub`), `ClaimTypes.Email` → `email`, `ClaimTypes.Name` → `name` (Google's
current display name).

**`OAuth2Service.HandleGoogleCallbackAsync`**:

1. Looks up a `User` by `Provider = Google` + `ProviderId`.
2. **First login (user not found):**
   - If the email already belongs to a **local** account, rejects with `ProviderMismatch` (this
     prevents hijacking a local account by logging in with Google using the same email).
   - Otherwise creates a `User` with `Status = Active` (no activation step — Google already
     verified the email) and no `Username`/`PasswordHash`.
   - Creates the linked `Person` using Google's `name` as-is.
3. **Returning login (user found):** fetches the `Person` and, if Google's current `name` differs
   from the stored one, calls `Person.UpdateName(name)` — keeps the profile in sync with the
   Google account without a separate request. No email/national-id/phone touched.
4. Revokes previous sessions, issues a JWT (using the **fresh** `name` from this login, not
   necessarily what's stored — though after step 3 they match), persists the new session.
5. Returns a `LoginResponse`; the cookie is set by the same handler that called this method.

**Important constraint:** Google-provisioned users have `PasswordHash = null` and
`Username = null` for life (nothing currently lets them set either). Their only entry point
remains the Google button — they cannot use `/login` with email/username + password.

## 6. Logout — `POST /api/auth/logout` (requires JWT)

Reads the token from the `access_token` cookie (falling back to the `Authorization: Bearer`
header), revokes the matching `UserSession` (`SessionNotFound` if missing/already revoked),
deletes the cookie, returns `200 OK`.

## 7. Token validation — `GET /api/auth/validate-token` (requires JWT)

Confirms the session tied to the current token is still active in `user_sessions` (not just that
the JWT signature is valid) — catches server-side revocation before the JWT's own expiration.
Returns `ValidateTokenResponse { Token }` or `401` if revoked/missing.

## 8. Identity — `GET /api/auth/me` (requires JWT)

Reads identity straight from JWT claims, **no database hit**: `sub` → `UserId`, `name` → `Name`,
`email` → `Email`, `role` → `Role`. Returns `MeResponse`. Because `name` is read from the token
(not re-queried), it reflects whatever was true at the moment the token was issued — it won't
pick up a `Person.Name` change until the user logs in again.

---

## Token & session strategy

- JWTs are HMAC-SHA256 signed, carry `sub`, `name`, `email`, `role` claims, and expire after
  `Jwt:ExpirationMinutes` (default 60).
- Every issued JWT has a matching row in `user_sessions`, storing a SHA-256 hex hash of the raw
  token (never the token itself) plus its expiration and revocation state. This is what makes
  server-side revocation possible — `validate-token` checks this table, not just the JWT
  signature.
- Logging in (local or Google) revokes **all** previously active sessions for that user — one
  active session per user at a time.
- The token is read from the `access_token` HttpOnly cookie first, falling back to the
  `Authorization: Bearer` header — supports both browser and non-browser clients.

## Identifiers at a glance

| Identifier | Local accounts | Google accounts |
|---|---|---|
| `Email` | required, unique | required, unique |
| `Username` | optional, unique | never set |
| `PasswordHash` | required | always `null` |
| Login methods available | email+password, username+password | Google button only |
