# Plan 03 — Authentication & Multi-Tenant Context

## Goal

Implement JWT-based authentication, tenant resolution from the authenticated user (never from the client body), and tenant registration APIs — all compiling and testable with **zero errors**.

## Prerequisites

- [Plan 02](02-database-and-entities.md) completed.

## Out of Scope

- Full RBAC enforcement (Plan 11)
- Agent pipeline
- OpenAI
- Frontend login UI (Plan 08)

---

## Step 1 — Add NuGet Packages

```powershell
dotnet add src/LLMCoordination.Infrastructure package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/LLMCoordination.Infrastructure package BCrypt.Net-Next
```

---

## Step 2 — JWT Configuration

`appsettings.json`:

```json
{
  "Jwt": {
    "Issuer": "LLMCoordination",
    "Audience": "LLMCoordination",
    "SecretKey": "DEV-ONLY-CHANGE-IN-PRODUCTION-MIN-32-CHARS!!",
    "ExpiryMinutes": 60
  }
}
```

Register in `Infrastructure/DependencyInjection.cs`:

- `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)`
- `AddJwtBearer` with validation parameters
- `AddAuthorization()`

In `Program.cs`:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

---

## Step 3 — Create Auth Contracts

`Application/Contracts/`:

| Type | Fields |
|------|--------|
| `LoginRequest` | Email, Password |
| `LoginResponse` | Token, ExpiresAt, UserId, TenantId, Role, FullName |
| `RegisterTenantRequest` | TenantName, DomainType, AdminEmail, AdminPassword, AdminFullName |
| `RegisterTenantResponse` | TenantId, UserId, Token |

---

## Step 4 — Auth Service

`Application/Services/AuthService.cs`:

- `RegisterTenantAsync` — creates Tenant + Admin AppUser, hashes password with BCrypt
- `LoginAsync` — validates credentials, returns JWT with claims:
  - `sub` = userId
  - `tenant_id` = tenantId
  - `role` = user role
  - `email` = user email

---

## Step 5 — Tenant Context Service

`Application/Services/TenantContextService.cs`:

- `GetCurrentTenantId()` — reads from `HttpContext.User` claim `tenant_id`
- `GetCurrentUserId()` — reads `sub`
- `GetCurrentUserRole()` — reads `role`

**Rule:** Never accept `tenantId` from request body for secured endpoints.

---

## Step 6 — Controllers

### `AuthController` (`/api/auth`)

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/register-tenant` | Anonymous | Register tenant + admin |
| POST | `/login` | Anonymous | Login, return JWT |

### `TenantController` (`/api/tenant`)

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/me` | Required | Current tenant info from JWT |

All `[Authorize]` controllers use `[Authorize]` attribute.

---

## Step 7 — Seed Dev Data (Optional)

`Infrastructure/Data/DbInitializer.cs`:

- On startup in Development, seed one tenant + admin if none exist:
  - Email: `admin@demo.local`
  - Password: `Demo123!`
  - Tenant: `Demo Healthcare`

Call from `Program.cs` after `app.Build()`.

---

## Step 8 — Unit Tests

| Test | Assert |
|------|--------|
| `RegisterTenant_CreatesTenantAndUser` | Tenant and user in DB |
| `Login_ValidCredentials_ReturnsToken` | Token not empty |
| `Login_InvalidPassword_Returns401` | Unauthorized |
| `TenantMe_WithoutToken_Returns401` | Unauthorized |

Use InMemory DB + `WebApplicationFactory` for integration tests if preferred.

---

## Deliverables Checklist

- [ ] JWT auth configured and working
- [ ] Register tenant + login endpoints
- [ ] Tenant context resolved from JWT claims
- [ ] `GET /api/tenant/me` returns tenant for authenticated user
- [ ] Dev seed data (optional)
- [ ] All tests pass

---

## Build Verification (Must Pass)

```powershell
dotnet build
dotnet test
dotnet run --project src/LLMCoordination.Api

# Register (first run)
curl -X POST http://localhost:5xxx/api/auth/register-tenant -H "Content-Type: application/json" -d "{\"tenantName\":\"Test\",\"domainType\":0,\"adminEmail\":\"admin@test.com\",\"adminPassword\":\"Test123!\",\"adminFullName\":\"Admin\"}"

# Login
curl -X POST http://localhost:5xxx/api/auth/login -H "Content-Type: application/json" -d "{\"email\":\"admin@test.com\",\"password\":\"Test123!\"}"

# Tenant me (replace TOKEN)
curl http://localhost:5xxx/api/tenant/me -H "Authorization: Bearer TOKEN"
```

### Expected Results

| Check | Expected |
|-------|----------|
| Build | 0 errors |
| Register | 200/201 with token |
| Login | 200 with JWT |
| /tenant/me without token | 401 |
| /tenant/me with token | 200 with tenant info |

---

## Common Build Errors & Fixes

| Error | Fix |
|-------|-----|
| 401 on all endpoints | Ensure `UseAuthentication()` before `UseAuthorization()` |
| JWT validation fails | SecretKey length ≥ 32 chars; Issuer/Audience match |
| Claim not found | Use consistent claim type names in token generation |

---

## Next Plan

Proceed to [04-agent-contracts-and-services.md](04-agent-contracts-and-services.md).
