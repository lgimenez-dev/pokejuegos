# Configure the API

`PokeJuegos.Api/appsettings.json` is committed with placeholders in uppercase (`YOUR_DB_PASSWORD`,
`YOUR_GOOGLE_CLIENT_ID`, ...) that show which values you need. Provide the real values in one of two ways.

## Option A — `appsettings.Development.json` (git-ignored)

Create it next to `appsettings.json` with your values. In Development it overrides `appsettings.json`, and
`.gitignore` keeps it out of the repository:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=pokejuegos;Username=postgres;Password=YOUR_DB_PASSWORD"
  },
  "Jwt": {
    "Secret": "YOUR_RANDOM_SECRET_AT_LEAST_32_CHARACTERS_LONG"
  },
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  }
}
```

## Option B — environment variables

ASP.NET Core maps `__` to the `:` of the settings keys:

| Variable | Required | Example |
|---|---|---|
| `ConnectionStrings__Default` | Yes | `Host=localhost;Port=5432;Database=pokejuegos;Username=postgres;Password=...` |
| `Jwt__Secret` | Yes | At least 32 characters (HMAC-SHA256 key) |
| `Google__ClientId` | Only for Google sign-in | `....apps.googleusercontent.com` |
| `Google__ClientSecret` | Only for Google sign-in | `GOCSPX-...` |

```powershell
# PowerShell (current session)
$env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=pokejuegos;Username=postgres;Password=..."
$env:Jwt__Secret = "<random string, 32+ characters>"
```

A random JWT secret can be generated with `openssl rand -hex 32`, or in PowerShell with
`[Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))`.

## Non-secret settings

These are already set in `appsettings.json`. You only need to change them if you change the ports:

| Key | Default | Used for |
|---|---|---|
| `App:FrontendUrl` | `http://localhost:3000` | Base URL of the links in the emails (`PokeJuegos.Web`) |
| `App:MobileRedirectUri` | `pokejuegos://oauth` | Where the Google flow hands the token back to the Android app |

`PokeJuegos.Web/appsettings.json` only needs `Api:BaseUrl` (`http://localhost:5224/` by default).

## Google sign-in (optional)

1. In the [Google Cloud Console](https://console.cloud.google.com/apis/credentials), create an **OAuth client ID** of
   type *Web application*.
2. Add this **authorized redirect URI**:
   `http://localhost:5224/api/auth/oauth2/google/callback`
3. Put the client ID and secret in `Google:ClientId` / `Google:ClientSecret` (option A or B above).

Google never redirects to the app directly. It always comes back to the API, which then hands the token to the app.
On Android, that happens through the `pokejuegos://oauth` custom scheme. On Windows, it goes through a loopback
listener on `http://127.0.0.1:<random port>/callback` (RFC 8252), because `WebAuthenticator` isn't supported on WinUI.
The API only accepts those two return URLs.
