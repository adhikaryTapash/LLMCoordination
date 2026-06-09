# Plan 07 — Skill Registry & Tenant Mappings

## Goal

Seed global reusable skills, map tenant Swagger endpoints to skills, and upgrade discovery/routing to use the registry — **zero build errors**, still mock execution acceptable.

## Prerequisites

- [Plan 06](06-swagger-parser-and-registration.md) completed.

## Out of Scope

- OpenAI embeddings (use keyword/operationId matching for now)
- MCP mappings (Plan 09)
- Database skill mappings (Plan 10)

---

## Step 1 — Seed Global Skills

`Infrastructure/Data/SkillSeedData.cs` — seed on startup if `GlobalSkillMaster` empty:

### Core Global Skills (from [docs/03-SKILL-DESIGN.md](../docs/03-SKILL-DESIGN.md))

| SkillName | Category | ActionType |
|-----------|----------|------------|
| swagger.endpoint.search | Api | Read |
| swagger.endpoint.explain | Api | Read |
| api.query.execute | Api | Read |
| api.record.create | Api | Create |
| api.record.update | Api | Update |
| api.record.delete | Api | Delete |
| database.schema.search | Database | Read |
| database.sql.generate | Database | Read |
| mcp.tool.discovery | Mcp | Read |
| mcp.tool.execute | Mcp | Execute |
| analytics.trend.analysis | Analytics | Read |
| response.card.generate | Response | Read |

Also seed 2–3 intent examples per skill in `GlobalSkillIntentExample` (plain text, no embeddings yet).

Seed prompt templates and response schemas as minimal JSON placeholders.

---

## Step 2 — Skill Registry Service

`Application/Skills/SkillRegistryService.cs`:

- `GetAllEnabledSkillsAsync()`
- `GetSkillByNameAsync(string skillName)`
- `FindBestSkillForIntentAsync(string intentName)` — rule-based mapping:

```text
patient.list          → api.query.execute
appointment.list.*    → api.query.execute
*.create              → api.record.create
*.update              → api.record.update
*.delete              → api.record.delete
```

---

## Step 3 — Auto-Mapping Service

`Application/Skills/TenantSkillMappingService.cs`:

When Swagger document is registered (hook from Plan 06 or call here):

- For each endpoint, infer skill by HTTP method:
  - GET → `api.query.execute`
  - POST → `api.record.create`
  - PUT/PATCH → `api.record.update`
  - DELETE → `api.record.delete`
- Insert `TenantSkillEndpointMapping` with confidence score (e.g. 0.8 default)

Manual override API optional in this plan.

---

## Step 4 — SkillController

`Api/Controllers/SkillController.cs`:

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/skills` | List global skills |
| GET | `/api/skills/{skillName}` | Skill detail + examples |
| GET | `/api/skills/mappings/endpoints` | Tenant endpoint mappings |
| POST | `/api/skills/mappings/endpoints` | Manual map skill ↔ endpoint |

All tenant-scoped routes filter by JWT tenant.

---

## Step 5 — Update Agents

| Agent | Change |
|-------|--------|
| `SkillRouterAgent` | Use `SkillRegistryService` instead of hardcoded map |
| `EndpointDiscoveryAgent` | Query `TenantSkillEndpointMapping` joined with endpoints for selected skill |

---

## Step 6 — SkillExecutionService (Stub)

`Application/Skills/SkillExecutionService.cs`:

- Accepts skill name + endpoint + context
- Delegates to `ApiExecutionAgent` for API skills
- Returns `ToolExecutionResult`

Prepares structure for MCP/DB skills in Plans 09–10 without implementing them yet.

---

## Step 7 — Tests

| Test | Assert |
|------|--------|
| `SkillSeed_Creates12Skills` | Count ≥ 12 |
| `AutoMapping_GET_MapsToApiQueryExecute` | Mapping exists |
| `SkillRouter_UsesRegistry` | Correct skill for intent |
| `EndpointDiscovery_UsesMapping` | Returns mapped endpoint |

---

## Deliverables Checklist

- [ ] Global skills seeded
- [ ] Intent examples seeded
- [ ] SkillRegistryService + TenantSkillMappingService
- [ ] SkillController APIs
- [ ] Agents use registry + mappings
- [ ] All tests pass

---

## Build Verification (Must Pass)

```powershell
dotnet build
dotnet test
dotnet run --project src/LLMCoordination.Api

curl http://localhost:5xxx/api/skills -H "Authorization: Bearer TOKEN"
curl http://localhost:5xxx/api/skills/mappings/endpoints -H "Authorization: Bearer TOKEN"

# Chat should still return success
curl -X POST http://localhost:5xxx/api/chat/message -H "Authorization: Bearer TOKEN" -H "Content-Type: application/json" -d "{\"message\":\"Give me the list of patients in card view\"}"
```

### Expected Results

| Check | Expected |
|-------|----------|
| Build | 0 errors |
| Tests | All pass |
| GET /api/skills | Returns seeded skills |
| Chat | success with skill=api.query.execute |

---

## Common Build Errors & Fixes

| Error | Fix |
|-------|-----|
| Duplicate seed on restart | Check `Any()` before seeding |
| FK violation on mapping | Ensure endpoint exists before mapping |
| Skill not found | Seed runs before first API call |

---

## Next Plans

- [08-frontend-foundation-and-chat-ui.md](08-frontend-foundation-and-chat-ui.md) (if not done)
- [09-mcp-integration.md](09-mcp-integration.md)
- [10-database-assistant.md](10-database-assistant.md)
