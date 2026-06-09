# Plan 04 — Agent Contracts & Service Wiring

## Goal

Define all agent interfaces and shared contracts from [docs/02-AGENTS.md](../docs/02-AGENTS.md) and [docs/05-API-CONTRACTS.md](../docs/05-API-CONTRACTS.md), implement **stub agents** that return mock data, and register everything in DI — **zero build errors**, no OpenAI yet.

## Prerequisites

- [Plan 03](03-authentication-and-tenants.md) completed.

## Out of Scope

- Real OpenAI / NLU
- Real API execution against tenant Swagger
- ChatController (Plan 05)
- Database writes from agents (except audit stub)

---

## Step 1 — Core Contracts

Create in `Application/Contracts/`:

### Agent Pipeline

| Type | Purpose |
|------|---------|
| `AgentContext` | ConversationId, TurnId, TenantId, UserId, UserRole, RawMessage, ViewPreference, TenantSettings |
| `AgentResult` | AgentName, Status, OutputJson, NextAgentHint, ErrorMessage |
| `IAgent` | `Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken ct)` |

### Intent & NLU

| Type | Key Fields |
|------|------------|
| `IntentResult` | Name, Confidence, Domain, Resource, Action |
| `EntityResult` | Operation, ViewType, Filters, Sort, Limit, Patient/customer entities |
| `NluAgentResponse` | Intent, Entities, RoutingHints, Clarification |

Match shapes from [docs/05-API-CONTRACTS.md](../docs/05-API-CONTRACTS.md).

### Skills & Tools

| Type | Key Fields |
|------|------------|
| `SkillDefinition` | SkillName, Domain, Category, Description, ActionType |
| `SkillRouterResult` | SelectedSkill, Reason |
| `EndpointDiscoveryResult` | Method, Path, OperationId, Confidence |
| `ToolExecutionRequest` | ToolType, ToolName, Method, Path, Parameters |
| `ToolExecutionResult` | Status, RawJson, RecordCount |

### Chat DTOs

| Type | Key Fields |
|------|------------|
| `ChatRequest` | ConversationId, Message, ViewPreference |
| `ChatResponse` | ConversationId, MessageId, Intent, Skill, ViewType, Status, Answer, Data |
| `CardResponse` | TotalRecords, Cards[] |
| `CardItem` | Title, Subtitle, Fields[], Actions[] |
| `TableResponse` | TotalRecords, Columns[], Rows[] |

---

## Step 2 — MVP Agent Stubs

Implement in `Application/Agents/` as injectable services implementing `IAgent` or dedicated interfaces:

| Agent | Stub Behavior |
|-------|---------------|
| `MasterOrchestratorAgent` | Calls agents in sequence; returns composed result |
| `NluIntentAgent` | Pattern-match keywords: "patient" + "list" → `patient.list` |
| `TenantContextAgent` | Load tenant from DB by TenantId in context |
| `SecurityRbacAgent` | Always allow `read`; block `delete` unless approval flag |
| `SkillRouterAgent` | Map `patient.list` → `api.query.execute` |
| `EndpointDiscoveryAgent` | Return mock `GET /api/patients` |
| `ApiExecutionAgent` | Return mock JSON patient array (3 records) |
| `ResponseComposerAgent` | Transform mock data to card/table based on ViewType |
| `AuditTelemetryAgent` | Log to `ILogger`; optionally write `AgentAuditLog` |

Each agent: **no external HTTP calls**, **no OpenAI**.

---

## Step 3 — Orchestration Service

`Application/Services/ChatOrchestrationService.cs`:

```text
ExecuteAsync(ChatRequest, AgentContext) → ChatResponse
  1. NluIntentAgent
  2. TenantContextAgent
  3. SecurityRbacAgent
  4. SkillRouterAgent
  5. EndpointDiscoveryAgent
  6. ApiExecutionAgent
  7. ResponseComposerAgent
  8. AuditTelemetryAgent
```

Return fully populated `ChatResponse` matching API contract.

---

## Step 4 — Register DI

`Application/DependencyInjection.cs`:

```csharp
services.AddScoped<MasterOrchestratorAgent>();
services.AddScoped<NluIntentAgent>();
// ... all MVP agents
services.AddScoped<ChatOrchestrationService>();
```

---

## Step 5 — Unit Tests

| Test | Assert |
|------|--------|
| `NluIntentAgent_PatientList_ReturnsCorrectIntent` | intent.name = patient.list |
| `SkillRouterAgent_ListOperation_SelectsApiQueryExecute` | skill = api.query.execute |
| `ResponseComposerAgent_CardView_ReturnsCards` | data.cards.length > 0 |
| `ChatOrchestrationService_EndToEnd_ReturnsSuccess` | status = success |

All tests use mocks/InMemory DB — no network.

---

## Deliverables Checklist

- [ ] All contract classes defined
- [ ] 9 MVP agent stubs implemented
- [ ] `ChatOrchestrationService` wires agents in order
- [ ] All agents registered in DI
- [ ] Unit tests pass
- [ ] No OpenAI package references yet

---

## Build Verification (Must Pass)

```powershell
dotnet build
dotnet test
```

### Expected Results

| Check | Expected |
|-------|----------|
| Build | 0 errors |
| Tests | All pass (≥ 4 agent tests) |
| No runtime required | This plan has no new HTTP endpoints yet |

---

## Common Build Errors & Fixes

| Error | Fix |
|-------|-----|
| Circular dependency between agents | Orchestrator injects agents; agents don't inject orchestrator |
| Nullable reference warnings as errors | Initialize required string properties |
| JSON serialization of `Data` object | Use `JsonElement` or `object` with `[JsonExtensionData]` |

---

## Next Plan

Proceed to [05-chat-api-mvp.md](05-chat-api-mvp.md).
