# SkillSnap

A full-stack portfolio management application built with Blazor WebAssembly, ASP.NET Core Web API, Entity Framework Core, and ASP.NET Identity.

---

## Project Summary

SkillSnap lets a user create a personal portfolio by managing their **projects** and **skills**. Authenticated users can add new entries; all visitors can browse the portfolio. The app demonstrates a complete, production-oriented .NET 9 solution with security, caching, state management, and clean API design.

---

## Architecture

```
SkillSnap.sln
├── Server/   ASP.NET Core Web API + Blazor host
├── Client/   Blazor WebAssembly SPA
└── Shared/   Models and DTOs shared between client and server
```

| Layer | Technology |
|---|---|
| Frontend | Blazor WebAssembly (.NET 9) |
| Backend | ASP.NET Core Web API (.NET 9) |
| Database | Entity Framework Core 9 + SQLite |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Client storage | Blazored.LocalStorage |
| Caching | IMemoryCache (server-side) |

---

## Key Features

### CRUD
- **Projects** — full Create / Read (list + detail) with server-side validation
- **Skills** — Create / Read with level tagging
- **Portfolio Users** — Read (profile view with related skills and projects)
- **Seed endpoint** (`POST /api/seed`) — inserts sample data for demo purposes

### Security
- JWT authentication via ASP.NET Core Identity (`UserManager`, `SignInManager`)
- All write endpoints protected with `[Authorize]`
- Token validated on every request: issuer, audience, lifetime, and signing key
- CORS locked to explicit origins from `appsettings.json`
- Consistent `UnauthorizedMessage` component shown to unauthenticated users on protected pages
- Token expiry checked client-side (JWT `exp` claim parsed in `UserSessionService`)
- Safe config reads — missing `Jwt:Key` or `Jwt:ExpiryMinutes` produce clear errors rather than silent failures

### Caching
- `IMemoryCache` with 5-minute sliding expiration on all three list endpoints (`/api/Projects`, `/api/Skills`, `/api/PortfolioUsers`)
- Cache invalidated on every write operation (POST / PUT / DELETE)

### State Management
- `UserSessionService` (scoped) parses JWT claims client-side (no extra round-trip)
- Stores `UserId`, `Email`, `Role`, and transient editing state (`ActiveProjectId`, `ActiveSkillId`)
- `AuthService.AuthStateChanged` event drives immediate NavMenu re-render on login/logout — no page refresh needed

### Performance
- `AsNoTracking()` on all read queries
- `Select` projections eliminate object cycles and over-fetching
- `async/await` throughout (no thread-blocking EF calls)
- `RequestTimingMiddleware` measures every request with `Stopwatch`, logs duration, and adds `X-Response-Time-Ms` response header

### Developer Experience
- Swagger UI available at `/swagger` in development
- `ServiceExtensions` organises startup into `AddSkillSnapData`, `AddSkillSnapAuth`, `AddSkillSnapCors`
- XML doc comments on all public service members

---

## Development Process and Use of GitHub Copilot

The project was built iteratively inside VS Code using **GitHub Copilot** as the primary coding assistant throughout every phase:

1. **Scaffolding** — Copilot generated the initial data models (`PortfolioUser`, `Project`, `Skill`), `SkillSnapContext` with Identity, and EF migrations.
2. **API layer** — Controllers, CORS policy, JWT setup, and the `ServiceExtensions` pattern were produced and refined through Copilot prompts, with the assistant catching issues such as circular JSON references and missing `UseAuthentication()` middleware.
3. **Client services** — `AuthService`, `ProjectService`, `SkillService`, and `PortfolioUserService` were generated and iteratively fixed (e.g. surfacing HTTP errors instead of swallowing them silently).
4. **Bug fixing** — Copilot diagnosed and fixed runtime issues including FK constraint failures, `required` keyword incompatibility with Blazor model binding, EF expression-tree null-propagation errors, and JWT object-cycle serialisation errors.
5. **Feature additions** — Caching, `RequestTimingMiddleware`, `UserSessionService`, the `UnauthorizedMessage` component, and the NavMenu auth-state reactivity pattern were all driven by focused prompts.
6. **Cleanup and polish** — Copilot identified unused components (`ProjectCard`, `SkillTags`), inconsistent return types across services, redundant `@using` directives, and synchronous EF calls, and applied fixes across multiple files simultaneously.

---

## Running the App

```bash
# From the solution root
cd Server
dotnet run
```

Then open `https://localhost:7232` in a browser.

**First-time setup:**
1. Navigate to `/swagger`
2. Call `POST /api/seed` to insert sample portfolio data
3. Call `POST /api/auth/register` to create a user account
4. Log in via the `/login` page

---

## Known Issues

| Issue | Detail |
|---|---|
| Single portfolio user | The Profile page loads `FirstOrDefault()` — designed for a single-user portfolio; no multi-user switching UI |
| No token refresh | When the JWT expires the user is silently shown as logged out on next render; there is no silent refresh flow |
| LocalStorage token | JWT stored in `localStorage` is readable by JavaScript (XSS risk). Acceptable for a portfolio app; a production app should consider HttpOnly cookies |
| No pagination | Project and skill lists fetch all records; large datasets would need server-side paging |
| Admin role not assigned | `[Authorize(Roles="Admin")]` guards exist on PUT/DELETE but no UI or endpoint exists to assign the Admin role |

---

## Future Improvements

- [ ] Edit and delete UI for projects and skills (Admin role flow)
- [ ] Multi-user profile selector on the Profile page  
- [ ] JWT silent refresh with a refresh-token endpoint
- [ ] Pagination on list endpoints
- [ ] Image upload (replace URL input with file upload to blob storage)
- [ ] Unit and integration tests (xUnit + bUnit)
- [ ] Deploy to Azure (App Service + Azure SQL)
