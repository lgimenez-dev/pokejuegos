# Architecture

PokeJuegos is a single .NET 10 solution with three runnable apps and five backend libraries.

## How the pieces fit together

```
                 ┌──────────────────────────┐
                 │ PokeJuegos.Mobile (MAUI) │──┐
                 └──────────────────────────┘  │  HTTP + JWT
┌────────────────┐   links in emails           ▼
│ PokeJuegos.Web │ ───────────────────► ┌────────────────┐      ┌────────────┐
│ (Razor Pages)  │ ──── HTTP ─────────► │ PokeJuegos.Api │ ───► │ PostgreSQL │
└────────────────┘                      └────────────────┘      └────────────┘
```

| Project | What it is |
|---|---|
| `PokeJuegos.Mobile` | .NET MAUI app (Android + Windows). Pokédex, roulette game, local top scores and favorites, login/register/Google sign-in against the API |
| `PokeJuegos.Api` | ASP.NET Core Web API (`http://localhost:5224`): registration with email activation, login, Google OAuth2, password recovery, server-side JWT sessions, SMTP config |
| `PokeJuegos.Web` | Razor Pages site (`http://localhost:3000`) with the `/activate` and `/reset-password` pages linked from the emails. Talks to the API over HTTP and only references `PokeJuegos.DTO` |
| `PokeJuegos.Common` / `DTO` / `Domain` / `Application` / `Infrastructure` | Backend layers (see [Backend](#backend-clean-architecture)). `PokeJuegos.DTO` is shared with the mobile app and the web |

## Tech stack

| Concern | Technology |
|---|---|
| Runtime | .NET 10, C# |
| Mobile / desktop | .NET MAUI (Android, Windows/WinUI), CommunityToolkit.Mvvm, CommunityToolkit.Maui, sqlite-net-pcl |
| API | ASP.NET Core, JWT Bearer, Google OAuth2, rate limiting, Swagger (Swashbuckle) |
| Web | ASP.NET Core Razor Pages |
| Data | PostgreSQL + Entity Framework Core (Npgsql) on the server, SQLite on the device |
| Auth | BCrypt password hashing, JWTs tracked server-side in `user_sessions`, tokens stored in `SecureStorage` on the device |
| Email | MailKit (SMTP) |
| Validation | FluentValidation |
| Data generation | Python scripts that pull the Pokémon list, details and sprites from [PokeAPI](https://pokeapi.co/) |

## Backend: Clean Architecture

The backend follows Clean Architecture with a strict one-way dependency rule:

```
Common ← DTO ← Domain ← Application ← Api
                              ↑            ↑
                    Infrastructure ────────┘
```

| Project | Responsibility | Main folders |
|---|---|---|
| `PokeJuegos.Common` | Enums (`UserRole`, `UserStatus`, `UserProvider`), helpers, `Optional<T>`. No dependencies | `Enums/`, `Helpers/` |
| `PokeJuegos.DTO` | Request/response records. No reference to Domain, so the mobile app and the web can reuse it | `Auth/`, `SmtpConfig/`, `UserSessions/` |
| `PokeJuegos.Domain` | Entities, repository interfaces, `Result<T>`, `IUnitOfWork` | `Entities/`, `Repositories/` |
| `PokeJuegos.Application` | Services and validators per feature, error codes (`ErrorMessages`), email templates, contracts for infrastructure services | `Auth/`, `OAuth2/`, `Sessions/`, `SmtpConfig/`, `UserConfig/`, `Services/`, `Helpers/` |
| `PokeJuegos.Infrastructure` | EF Core `DbContext` and configurations, repositories, JWT, BCrypt and MailKit implementations | `Configurations/`, `Repositories/`, `Services/` |
| `PokeJuegos.Api` | Controllers, middleware, DI wiring. No business logic | `Controllers/`, `Extensions/` (JWT, Google, Swagger, CORS), `Middleware/`, `Models/`, `Helpers/` |

The database schema is managed with plain SQL scripts in `db/` (no EF migrations). See
[Create the database](CREATE_DATABASE.md) and [Database design](../db/DESIGN.md).

## Mobile app: MVVM

The app uses MVVM with CommunityToolkit.Mvvm (source-generated observable properties and commands), compiled XAML
bindings (`x:DataType` everywhere, so the trimmed Release APK behaves like Debug), Shell navigation behind an
`INavigationService`, and constructor injection for every page and view model. Platform-specific pieces are
registered per target (`#if ANDROID` / `#elif WINDOWS`), such as the Google sign-in browser.

| Folder | Contents |
|---|---|
| `Views/` · `ViewModels/` | Welcome, Login, Register, ForgotPassword, CompleteProfile, General, Pokedex, Roulette |
| `Controls/` | Reusable controls (ModalView, FormEntry, SearchBox, OptionPicker, PokemonCard, ...) and drawn ones (BurstBackground, FuseBar, ConfettiView on `GraphicsView`) |
| `Services/` | HttpAuthService, SessionService (`SecureStorage`), LocalDatabase (SQLite), favorites, top scores, navigation |
| `Models/` | App models, SQLite tables, source-generated JSON context |
| `Platforms/` | Android (WebAuthenticator callback) and Windows (loopback OAuth browser) |
| `Resources/` | Styles and colors, SVG icons, sprites, fonts, `Raw/pokemon_list.json` + `pokemon_details.json` (PokeAPI shape, gen I–III) |
| `scripts/` | Python scripts that generate the Pokémon data and sprites from PokeAPI |

## Repository layout

```
PokeJuegos/
├── docs/                        Architecture, setup guides, API reference, authentication flow
├── db/                          script.sql (schema), seed.sql, query.sql (reference queries), DESIGN.md
├── screencap/                   Screenshots used in the README
├── PokeJuegos.Api/
├── PokeJuegos.Application/
├── PokeJuegos.Infrastructure/
├── PokeJuegos.Domain/
├── PokeJuegos.DTO/
├── PokeJuegos.Common/
├── PokeJuegos.Web/
├── PokeJuegos.Mobile/
├── docker-compose.yml           PostgreSQL + API + Web (each with its own Dockerfile)
├── PokeJuegos.sln
└── PokeJuegos.slnLaunch         Shared Visual Studio launch profiles (API + Web + app)
```
