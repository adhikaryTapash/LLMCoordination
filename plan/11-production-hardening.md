# Plan 11 — Production Hardening

## Goal

Add audit logging, RBAC enforcement, rate limiting, error recovery, OpenAI integration (optional switch), and full smoke tests — entire solution builds with **zero errors** and passes integration tests.

## Prerequisites

- [Plan 09](09-mcp-integration.md) completed
- [Plan 10](10-database-assistant.md) completed
- [Plan 08](08-frontend-foundation-and-chat-ui.md) recommended

## Out of Scope

- Azure Key Vault deployment (document config only)
- Full analytics dashboard UI
- pgvector production tuning

---

## Step 1 — Audit & Telemetry (Full)

Upgrade `AuditTelemetryAgent`:

- Persist every chat turn to `AgentAuditLog`
- Fields: intent, skill, toolType, toolName, request/response JSON, status, cost, executionMs
- Never store secrets or raw credentials

`Api/Controllers/AuditController.cs`:

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/audit/logs` | Paginated audit logs (Admin only) |

Frontend: wire `AuditLogPage.tsx` to this API (optional but must compile if touched).

---

## Step 2 — RBAC Enforcement

Implement full `SecurityRbacAgent` using `TenantPermissionRule`:

| Role | Read | Create | Update | Delete | Export |
|------|------|--------|--------|--------|--------|
| Admin | ✓ | ✓ | ✓ | approval | ✓ |
| User | ✓ | approval | approval | ✗ | ✗ |
| Viewer | ✓ | ✗ | ✗ | ✗ | ✗ |

Seed default rules per tenant on registration.

Block chat execution early with clear `403` or clarification message.

---

## Step 3 — Approval / HITL Agent

`ApprovalAgent`:

- POST/PATCH/DELETE API skills and write MCP tools return:

```json
{
  "status": "approval_required",
  "answer": "This action requires approval.",
  "data": { "approvalToken": "...", "action": "api.record.delete" }
}
```

Add `POST /api/chat/approve` to resume with `approvalToken`.

Frontend: show Approve/Deny buttons when status is `approval_required`.

---

## Step 4 — Error Recovery Agent

`ErrorRecoveryAgent`:

- Catch API timeout, 404 endpoint, missing parameters
- Max 1 retry for transient errors
- Return user-friendly message via ResponseComposerAgent

Register in orchestrator as try/catch wrapper.

---

## Step 5 — Rate Limiting

```powershell
dotnet add src/LLMCoordination.Api package Microsoft.AspNetCore.RateLimiting
```

Policy:

- Chat endpoint: 20 requests/minute per user
- Register endpoints: 10 requests/hour per tenant

Return `429 Too Many Requests` with Retry-After header.

---

## Step 6 — OpenAI Integration (Feature Flag)

`appsettings.json`:

```json
{
  "OpenAI": {
    "Enabled": false,
    "ApiKey": "",
    "Model": "gpt-4o-mini"
  }
}
```

`Infrastructure/OpenAI/OpenAiNluService.cs`:

- When `Enabled=true`: real intent extraction
- When `Enabled=false`: existing keyword stub (Plan 04)

**Rule:** OpenAI never executes tools — only returns structured intent/plan JSON.

```powershell
dotnet add src/LLMCoordination.Infrastructure package Azure.AI.OpenAI
```

Guard: if Enabled=true but ApiKey empty → log warning, fall back to stub (no crash).

---

## Step 7 — Real API Execution

Upgrade `ApiExecutionAgent`:

- Build HttpClient request from discovered endpoint
- Pass query params from NLU entities
- Use tenant-stored auth from Swagger document config (Bearer)
- Mask sensitive fields in response (password, ssn, token)

Dev: if tenant API unreachable, fall back to mock with logged warning.

---

## Step 8 — Conversation Persistence

Ensure all chat turns saved:

- `Conversation` + `ConversationMessage` for user and assistant
- Link audit logs via ConversationId + TurnId

---

## Step 9 — Structured Logging & Health

- Serilog or built-in logging with correlation ID per request
- Extend `/api/health` with checks: DB, OpenAI config (not call), version

---

## Step 10 — Configuration Security Checklist

Document in code comments / config:

- [ ] JWT secret from environment variable in production
- [ ] Encryption key from Key Vault
- [ ] Connection strings from environment
- [ ] OpenAI key never in frontend
- [ ] CORS restricted to known origins in production

---

## Step 11 — Full Test Suite

| Area | Tests |
|------|-------|
| Auth | Login, tenant isolation |
| Chat | Card/table/text responses |
| Swagger | Parse, register, map |
| Skills | Registry, mappings |
| MCP | Register, discover, encrypt |
| Database | SQL validation, read-only |
| RBAC | Viewer cannot delete |
| Audit | Log written per chat |
| Rate limit | 429 after threshold |

Target: **`dotnet test` all green**.

---

## Step 12 — Frontend Finalization

If any hardening UI added:

- Approval buttons on chat messages
- Audit log table on AuditLogPage
- Error toast for 429/403

Run:

```powershell
cd client
npm run build
```

---

## Deliverables Checklist

- [ ] Full audit persistence + API
- [ ] RBAC enforced on all execution paths
- [ ] Approval flow for write operations
- [ ] Error recovery with retry
- [ ] Rate limiting on chat/register
- [ ] OpenAI behind feature flag (optional)
- [ ] Real API execution with fallback
- [ ] Conversation persistence
- [ ] Full test suite passes
- [ ] Frontend builds

---

## Build Verification (Must Pass)

```powershell
# Backend
dotnet build
dotnet test

# Frontend
cd client
npm run build

# Smoke test script (manual)
dotnet run --project src/LLMCoordination.Api
# 1. Login
# 2. Register swagger
# 3. Chat patient list → cards
# 4. Chat delete patient → approval_required
# 5. GET audit logs as admin
# 6. Verify viewer role blocked from delete
```

### Expected Results

| Check | Expected |
|-------|----------|
| `dotnet build` | 0 errors |
| `dotnet test` | 0 failures |
| `npm run build` | 0 errors |
| RBAC | Viewer delete blocked |
| Audit | ≥ 1 log entry per chat |
| Rate limit | 429 after limit |

---

## Common Build Errors & Fixes

| Error | Fix |
|-------|-----|
| OpenAI package fails without key | Feature flag off by default |
| Rate limiting breaks tests | Disable or raise limits in test environment |
| Approval flow breaks chat types | Extend ChatResponse status enum |
| Frontend TS errors on approval UI | Add optional fields to chatTypes |

---

## Project Complete

After Plan 11, the MVP matches the blueprint in [docs/README.md](../docs/README.md):

- Multi-tenant auth
- Swagger + MCP + DB registration
- Global skills with tenant tool mappings
- Agent orchestration with safe execution
- Structured chat responses with card/table views
- Audit, RBAC, and production safeguards

Future enhancements (post-MVP):

- pgvector semantic search
- Streaming chat responses
- Azure deployment pipelines
- Admin analytics dashboard
