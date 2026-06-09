# Plan 06 — Swagger Parser & Registration

## Goal

Allow tenants to register Swagger/OpenAPI documents, parse endpoints, store them in the database, and list them via API — **zero build errors**, no vector search yet.

## Prerequisites

- [Plan 05](05-chat-api-mvp.md) completed.

## Out of Scope

- pgvector embeddings (Plan 07)
- Real endpoint discovery via semantic search (Plan 07)
- Executing real tenant API calls (wire in Plan 07)

---

## Step 1 — Add NuGet Packages

```powershell
dotnet add src/LLMCoordination.Infrastructure package Microsoft.OpenApi.Readers
```

Or use `System.Text.Json` manual parsing if avoiding extra dependency — prefer OpenAPI.NET for correctness.

---

## Step 2 — Swagger Contracts

`Application/Contracts/`:

| Type | Fields |
|------|--------|
| `RegisterSwaggerRequest` | Name, SwaggerUrl OR RawJson, Domain |
| `SwaggerDocumentDto` | Id, Name, Version, Status, EndpointCount |
| `SwaggerEndpointDto` | Id, Method, Path, OperationId, Summary, Description, Tags |

---

## Step 3 — SwaggerParserService

`Application/Services/SwaggerParserService.cs`:

Input: OpenAPI JSON string  
Output: List of parsed endpoints with:

- method, path, operationId, summary, description
- requestSchemaJson, responseSchemaJson (serialized)
- authRequired (from security requirements)
- tags

Handle OpenAPI 3.0.x. Invalid JSON → throw typed `SwaggerParseException`.

---

## Step 4 — Swagger Registration Flow

`Application/Services/SwaggerRegistrationService.cs`:

1. Validate tenant from JWT
2. Fetch JSON from URL (HttpClient) OR accept uploaded raw JSON
3. Parse via `SwaggerParserService`
4. Save `TenantSwaggerDocument` + bulk insert `TenantSwaggerEndpoint`
5. Return document summary

**Security:** Only fetch URLs allowed by tenant admin; timeout 30s; no SSRF to internal IPs in production (add URL validation stub for now).

---

## Step 5 — SwaggerController

`Api/Controllers/SwaggerController.cs` — all `[Authorize]`:

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/swagger/register` | Register by URL or raw JSON |
| GET | `/api/swagger/documents` | List tenant documents |
| GET | `/api/swagger/documents/{id}/endpoints` | List parsed endpoints |
| DELETE | `/api/swagger/documents/{id}` | Soft-delete or hard-delete document + endpoints |

All queries filter by `TenantId` from JWT.

---

## Step 6 — Sample Swagger Fixture for Tests

`tests/LLMCoordination.Tests/Fixtures/sample-openapi.json`:

Minimal OpenAPI with:

- `GET /api/patients`
- `GET /api/patients/{id}`
- `POST /api/patients`

---

## Step 7 — Unit & Integration Tests

| Test | Assert |
|------|--------|
| `SwaggerParser_ExtractsEndpoints` | 3 endpoints parsed |
| `RegisterSwagger_StoresInDatabase` | Endpoints linked to tenant |
| `ListEndpoints_FiltersByTenant` | Tenant A cannot see Tenant B |
| `InvalidJson_Returns400` | Bad request |

---

## Step 8 — Update EndpointDiscoveryAgent (Minimal)

Change stub to:

1. Query `TenantSwaggerEndpoints` for current tenant
2. Simple keyword match: "patient" → path contains `patient`
3. Fallback to first GET endpoint

Still no OpenAI — deterministic matching only.

---

## Deliverables Checklist

- [ ] SwaggerParserService parses OpenAPI JSON
- [ ] Register/list/delete Swagger APIs
- [ ] Tenant isolation on all queries
- [ ] Sample fixture + tests pass
- [ ] EndpointDiscoveryAgent reads from DB

---

## Build Verification (Must Pass)

```powershell
dotnet build
dotnet test
dotnet run --project src/LLMCoordination.Api

# Register sample swagger (inline JSON or file upload)
curl -X POST http://localhost:5xxx/api/swagger/register -H "Authorization: Bearer TOKEN" -H "Content-Type: application/json" -d @sample-register.json

curl http://localhost:5xxx/api/swagger/documents -H "Authorization: Bearer TOKEN"
```

### Expected Results

| Check | Expected |
|-------|----------|
| Build | 0 errors |
| Tests | All pass |
| Register | Document + endpoints in DB |
| Chat still works | Patient list uses registered endpoint metadata |

---

## Common Build Errors & Fixes

| Error | Fix |
|-------|-----|
| OpenAPI parse null reference | Guard missing paths/operations |
| HttpClient deadlock | Use async/await throughout |
| Tenant leak | Always filter by TenantId from JWT |

---

## Next Plan

Proceed to [07-skill-registry-and-mappings.md](07-skill-registry-and-mappings.md).
