---
name: backend
description: Conventions and workflow for the PokeJuegos backend — PokeJuegos.Api (ASP.NET Core, .NET 10), the library projects (Common, DTO, Domain, Application, Infrastructure), PokeJuegos.Web (Razor Pages) and the PostgreSQL scripts in db/. Use whenever adding or changing endpoints, services, validators, entities, repositories, EF Core configurations, emails, settings or SQL in this solution.
---

# PokeJuegos backend

.NET 10, Clean Architecture, flat PostgreSQL (single `public` schema, no multi-tenancy). Everything in English:
code, XML comments, error messages, endpoints.

## Dependency rule (never break it)

```
Common ← DTO ← Domain ← Application ← Api
                              ↑            ↑
                    Infrastructure ────────┘
```

| Project | Holds | References |
|---|---|---|
| `PokeJuegos.Common` | Enums (`UserRole`, `UserStatus`, `UserProvider`), helpers, `Optional<T>` (`Of`/`Empty`) | nothing, no NuGet |
| `PokeJuegos.DTO` | Positional request/response records | Common. **Also used by Mobile and Web**: no Domain, no server-only types |
| `PokeJuegos.Domain` | Entities, repository interfaces (`Repositories/`), `Result<T>`, `IUnitOfWork`, `AuditableEntity` | Common, DTO. No NuGet |
| `PokeJuegos.Application` | `<Feature>/I<Feature>Service.cs` + `<Feature>Service.cs` + validators, `ErrorMessages`, `Helpers/` (Hmac, Email templates), `Services/` (infra contracts: `IJwtService`, `IPasswordService`, `IEmailService`) | Common, DTO, Domain. Interfaces only, never concrete infra |
| `PokeJuegos.Infrastructure` | `AppDbContext`, `Configurations/` (EF), `Repositories/`, `Services/` (JWT, BCrypt, MailKit), `UnitOfWork` | Common, Domain, Application |
| `PokeJuegos.Api` | Controllers, `Extensions/` (one file per concern), `Middleware/`, `Models/` (error responses), `Helpers/` | everything (DI wiring only, no business logic) |
| `PokeJuegos.Web` | Razor Pages `/activate`, `/reset-password`; calls the API over HTTP (`AuthApiClient`) | DTO only — never Application/Infrastructure |

No CQRS, MediatR, commands/queries or pipeline behaviors.

## Code conventions

- **Explicit types always.** No `var`, `dynamic` or `object` (only exception: anonymous types in LINQ projections).
- **XML `<summary>`** (with `<param>`/`<returns>`) on every public type, method, controller action and service method.
- **Braces always** (`.editorconfig`: `csharp_prefer_braces = true:warning`). 4-space indentation.
- **Primary constructors** for DI in controllers and services (`public class AuthController(IAuthService authService, ...)`).
- **YAGNI**: no logging, caching, base classes or helpers that weren't asked for.
- Feature folder namespace = folder name, except when it collides with an entity: use the plural
  (`Application/SmtpConfig/` → `PokeJuegos.Application.SmtpConfigs`).

### Result pattern

Services return `Result<T>`; never throw for business rules (`ExceptionHandlingMiddleware` handles the truly
unexpected). Prefer the implicit operators. Error strings always come from `ErrorMessages` constants.

```csharp
return true;                              // Success
return ErrorMessages.InvalidCredentials;  // Failure
```

### DTOs

Positional records, no `FromEntity`. Mapping happens in `private static` helpers inside the service.

### Entities

Inherit `AuditableEntity`; `private` parameterless constructor; `static Create(...)` factory that calls `InitAudit()`;
`private set` on every property; state changes are intent-named methods that call `SetUpdatedAt()`
(`Revoke()`, `ChangePassword(...)`, `RevokeToken()`). No EF attributes in Domain.

### EF Core (Infrastructure)

One `IEntityTypeConfiguration<T>` per entity in `Infrastructure/Configurations/`. Enums are PascalCase in C# and
stored UPPERCASE via a value converter configured there (`r => r.ToString().ToUpper()` / `Enum.Parse<T>(s, true)`).
New repositories: interface in `Domain/Repositories/`, implementation in `Infrastructure/Repositories/`, exposed on
`IUnitOfWork`. Register in `InfrastructureServiceExtensions.AddInfrastructure`.

### Controllers

HTTP only. Validators are injected and **called explicitly** (no auto-validation pipeline):

```csharp
ValidationResult validation = await registerValidator.ValidateAsync(request, ct);
if (!validation.IsValid)
{
    return BadRequest(ValidationErrorResponse.From(validation));
}

Result<bool> result = await authService.RegisterAsync(request, ct);
if (!result.IsSuccess)
{
    return BadRequest(new BusinessErrorResponse(result.Error!));
}

return Ok();
```

`Program.cs` stays declarative: everything goes through extension methods (`AddInfrastructure`, `AddApplication`,
`AddJwtBearerAuth`, `AddGoogleAuth`, `AddDefaultCors`, `AddSwaggerWithAuth`). New services register in
`ApplicationExtensions.AddApplication` (scoped). Rate limiting policies are per IP and reject with 429
(the app shows "too many attempts" on 429).

## Auth facts worth knowing

- JWTs are tracked in `user_sessions` (hashed); login revokes previous sessions. Token read from the `access_token`
  HttpOnly cookie, falling back to `Authorization: Bearer` (the mobile app uses the header).
- Register → `Pending` + email link `{App:FrontendUrl}/activate?token=` → `POST /api/auth/activate` (keeps the
  password chosen at registration; only activates `Pending` accounts). Recovery → `/reset-password?token=` →
  `POST /api/auth/change-password`. `/activate` activates on button press, never on page load (mail scanners).
- Google: callback `/api/auth/oauth2/google/callback`. Native clients pass a `returnUrl` validated by
  `Api/Helpers/OAuthReturnUrl` (only `App:MobileRedirectUri` or `http://127.0.0.1:<port>/callback`). Google users
  have no username until `PUT /api/auth/profile`. Google sign-in with an email owned by a password account is
  rejected.
- `recover-password` always answers 200 (no account enumeration).

## Emails (`Application/Helpers/EmailHelper.cs`)

White background, no card (no grey background, rounded borders or shadow). Centered bold title with divider,
full-width pill button `#F22539`, red links, Lato with Arial fallback, max 440px column, footer `© 2026 {appName}`
in `#aaaaaa`. Palette from `PokeJuegos.Mobile/Resources/Styles/Colors.xaml`.

## Configuration and secrets

- `PokeJuegos.Api/appsettings.json` is committed with UPPERCASE placeholders (`YOUR_DB_PASSWORD`, ...). Real values
  live in `appsettings.Development.json` (git-ignored). Do not introduce User Secrets or `.example` files.
- Never put credentials in a committed file (`db/seed.sql` uses placeholders; the real one is `db/seed.local.sql`,
  ignored). The repo is public.
- Ports: API `http://localhost:5224` (https 7062), Web `http://localhost:3000` (`App:FrontendUrl`).

## Database (`db/`)

No EF migrations: the schema is `db/script.sql`, initial data `db/seed.sql`, reference queries `db/query.sql`,
decisions in `db/DESIGN.md` (read it before touching `games`, `user_game_data` or `user_configs`). Keep script,
entities/configurations and DESIGN.md in sync. Local PostgreSQL 17: `C:\Program Files\PostgreSQL\17\bin\psql.exe`.
Test schema changes on a throwaway database and drop it afterwards.

## Workflow

- Discuss the design before editing when the change is structural (new tables, endpoints, flows); the user wants to
  confirm first. A question from the user is not a request to change things.
- Verify with `dotnet build` of the touched project(s), 0 errors / 0 warnings. If Visual Studio is running the API,
  its DLLs are locked: build only the project you changed (e.g. Application).
- If you start the API/Web to check something, first check the port: never kill the user's Visual Studio instance
  on 5224; use another port (`--urls http://localhost:5299`) and stop your process when done.
- Do not commit; the user handles git.
