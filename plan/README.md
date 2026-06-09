# VIBE AI — Implementation Plans

This folder breaks the [docs](../docs/README.md) blueprint into **11 buildable milestones**. Each plan ends in a **zero-error build** before moving to the next.

## Rules for Every Plan

1. **Complete the plan fully** before starting the next one.
2. **Run the verification commands** at the end of each plan.
3. **Do not skip ahead** — later plans depend on earlier contracts and folder structure.
4. **Use mocks first** — real OpenAI, PostgreSQL, and external APIs come only when a plan explicitly says so.
5. **Keep tenant isolation in mind** from Plan 03 onward, even if enforcement is minimal at first.

## Plan Index

| Plan | Title | Build Target | Depends On |
|------|-------|--------------|------------|
| [01](01-solution-foundation.md) | Solution & Project Foundation | `dotnet build` | — |
| [02](02-database-and-entities.md) | Database & EF Core Entities | `dotnet build` + migration | 01 |
| [03](03-authentication-and-tenants.md) | JWT Auth & Multi-Tenant Context | `dotnet build` + API smoke test | 02 |
| [04](04-agent-contracts-and-services.md) | Agent Contracts & DI Wiring | `dotnet build` | 03 |
| [05](05-chat-api-mvp.md) | Chat API with Mock Agents | `dotnet build` + POST /api/chat/message | 04 |
| [06](06-swagger-parser-and-registration.md) | Swagger Upload & Endpoint Catalog | `dotnet build` + Swagger API test | 05 |
| [07](07-skill-registry-and-mappings.md) | Global Skills & Tenant Mappings | `dotnet build` + seed verification | 06 |
| [08](08-frontend-foundation-and-chat-ui.md) | React Frontend & Chat UI | `npm run build` + API integration | 05 |
| [09](09-mcp-integration.md) | MCP Server Registration & Execution | `dotnet build` + MCP API test | 07 |
| [10](10-database-assistant.md) | DB Schema & Read-Only SQL | `dotnet build` + SQL safety tests | 07 |
| [11](11-production-hardening.md) | Audit, RBAC, Rate Limits, Hardening | `dotnet build` + full smoke test | 09, 10 |

## Recommended Execution Order

```text
01 → 02 → 03 → 04 → 05 → 06 → 07
                              ↓
                         08 (can start after 05)
                              ↓
                    09 and 10 (parallel after 07)
                              ↓
                            11
```

Plan **08** (frontend) can start after **05** because it only needs the Chat API contract. Plans **09** and **10** are independent and can run in parallel after **07**.

## Solution Naming

The docs use `VibeAI.*` project names. This repo is currently `LLMCoordination`. During Plan 01, rename or add projects using one consistent root name:

- **Option A (recommended):** Keep `LLMCoordination` as the solution name and use `LLMCoordination.Api`, `LLMCoordination.Application`, etc.
- **Option B:** Rename everything to `VibeAI.*` to match the docs exactly.

Pick one option in Plan 01 and use it in all later plans.

## Build Verification Cheat Sheet

```powershell
# Backend (run from solution root after every backend plan)
dotnet build
dotnet test

# Frontend (run from client/ after Plan 08)
npm install
npm run build
```

## Reference Docs

- Architecture: [docs/01-ARCHITECTURE.md](../docs/01-ARCHITECTURE.md)
- Agents: [docs/02-AGENTS.md](../docs/02-AGENTS.md)
- Skills: [docs/03-SKILL-DESIGN.md](../docs/03-SKILL-DESIGN.md)
- Database: [docs/04-DATABASE-SCHEMA.md](../docs/04-DATABASE-SCHEMA.md)
- API Contracts: [docs/05-API-CONTRACTS.md](../docs/05-API-CONTRACTS.md)
- Coding Flow: [docs/08-CODING-FLOW.md](../docs/08-CODING-FLOW.md)
- Cursor Prompts: [docs/10-CURSOR-PROMPTS.md](../docs/10-CURSOR-PROMPTS.md)
