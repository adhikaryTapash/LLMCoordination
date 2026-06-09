# Plan 01 — Solution & Project Foundation

## Goal

Restructure the single-project template into a clean layered solution that compiles with **zero errors** and no unfinished references.

## Prerequisites

- None (starting point).
- .NET 10 SDK installed.
- Current repo contains the default `WeatherForecast` template.

## Out of Scope (Do Not Add Yet)

- PostgreSQL / EF Core
- JWT auth
- OpenAI calls
- React frontend
- Agent logic

Adding any of the above in this plan risks build failures from missing packages or half-wired DI.

---

## Step 1 — Choose Naming Convention

Pick one and stick to it:

| Layer | Option A (keep repo name) | Option B (match docs) |
|-------|---------------------------|------------------------|
| API | `LLMCoordination.Api` | `VibeAI.Api` |
| Application | `LLMCoordination.Application` | `VibeAI.Application` |
| Domain | `LLMCoordination.Domain` | `VibeAI.Domain` |
| Infrastructure | `LLMCoordination.Infrastructure` | `VibeAI.Infrastructure` |
| Tests | `LLMCoordination.Tests` | `VibeAI.Tests` |

---

## Step 2 — Create Solution Structure

From the repo root:

```powershell
dotnet new sln -n LLMCoordination
dotnet new classlib -n LLMCoordination.Domain -o src/LLMCoordination.Domain
dotnet new classlib -n LLMCoordination.Application -o src/LLMCoordination.Application
dotnet new classlib -n LLMCoordination.Infrastructure -o src/LLMCoordination.Infrastructure
dotnet new webapi -n LLMCoordination.Api -o src/LLMCoordination.Api
dotnet new xunit -n LLMCoordination.Tests -o tests/LLMCoordination.Tests

dotnet sln add src/LLMCoordination.Domain
dotnet sln add src/LLMCoordination.Application
dotnet sln add src/LLMCoordination.Infrastructure
dotnet sln add src/LLMCoordination.Api
dotnet sln add tests/LLMCoordination.Tests
```

### Project References

```powershell
dotnet add src/LLMCoordination.Application reference src/LLMCoordination.Domain
dotnet add src/LLMCoordination.Infrastructure reference src/LLMCoordination.Application
dotnet add src/LLMCoordination.Infrastructure reference src/LLMCoordination.Domain
dotnet add src/LLMCoordination.Api reference src/LLMCoordination.Application
dotnet add src/LLMCoordination.Api reference src/LLMCoordination.Infrastructure
dotnet add tests/LLMCoordination.Tests reference src/LLMCoordination.Api
```

---

## Step 3 — Move & Clean Up Template Files

1. **Delete** root-level template files after migration:
   - `WeatherForecast.cs`
   - `Controllers/WeatherForecastController.cs`
   - Root `LLMCoordination.csproj` (replaced by `src/LLMCoordination.Api`)
2. **Remove** the old root web project from the solution if it still exists.
3. Keep `appsettings.json` only under `src/LLMCoordination.Api/`.

---

## Step 4 — Scaffold Empty Folders (Placeholder Files)

Create one empty or minimal file per folder so the structure exists and compiles. Use `internal` or empty classes — no business logic.

### Domain (`src/LLMCoordination.Domain/`)

```text
Entities/     → placeholder Tenant.cs (empty record or stub)
Enums/        → DomainType.cs (Healthcare, Erp, Mixed)
```

### Application (`src/LLMCoordination.Application/`)

```text
Agents/       → .gitkeep or empty IAgent.cs interface
Contracts/    → .gitkeep
Skills/       → .gitkeep
Services/     → .gitkeep
DependencyInjection.cs  → AddApplication() extension (empty body OK)
```

### Infrastructure (`src/LLMCoordination.Infrastructure/`)

```text
DependencyInjection.cs  → AddInfrastructure() extension (empty body OK)
```

### API (`src/LLMCoordination.Api/`)

```text
Controllers/
  HealthController.cs   → GET /api/health returns { "status": "ok" }
Program.cs              → wire AddApplication + AddInfrastructure
appsettings.json
appsettings.Development.json
```

---

## Step 5 — Wire Program.cs

`src/LLMCoordination.Api/Program.cs` must:

```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
```

Pipeline: OpenAPI in Development, HTTPS, Authorization (no auth yet), MapControllers.

---

## Step 6 — Add Health Endpoint

`HealthController`:

- Route: `GET /api/health`
- Response: `{ "status": "ok", "service": "LLMCoordination.Api" }`

This replaces WeatherForecast as the smoke-test endpoint.

---

## Step 7 — Configure Launch Settings

Update `Properties/launchSettings.json`:

- Application URL: `https://localhost:7xxx` / `http://localhost:5xxx`
- Document the port in this plan's notes for later frontend CORS config.

---

## Deliverables Checklist

- [ ] Solution with 5 projects (Domain, Application, Infrastructure, Api, Tests)
- [ ] No root-level web project remnants
- [ ] `HealthController` responds 200
- [ ] `DependencyInjection` extension methods exist (can be empty)
- [ ] One passing unit test: `HealthController_ReturnsOk` or solution builds with empty test project

---

## Build Verification (Must Pass)

```powershell
cd d:\tapash-adhikary\AidEun\Co-ordinate-system\LLMCoordinateSystemPatheFour\LLMCoordination
dotnet build
dotnet test
dotnet run --project src/LLMCoordination.Api
# In another terminal:
curl http://localhost:5xxx/api/health
```

### Expected Results

| Command | Expected |
|---------|----------|
| `dotnet build` | 0 errors, 0 warnings (warnings OK but aim for 0) |
| `dotnet test` | All tests pass (or 0 tests, 0 failures) |
| `GET /api/health` | HTTP 200, JSON status ok |

---

## Common Build Errors & Fixes

| Error | Fix |
|-------|-----|
| Duplicate Program.cs / csproj at root | Remove old root web project |
| Missing project reference | Re-run `dotnet add reference` commands |
| Namespace mismatch | Use `LLMCoordination.*` consistently |
| Api can't find Application DI | Ensure `AddApplication()` is called in Program.cs |

---

## Next Plan

Proceed to [02-database-and-entities.md](02-database-and-entities.md).
