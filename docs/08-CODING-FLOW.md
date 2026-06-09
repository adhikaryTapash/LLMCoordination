# Coding Flow

## Phase 1 - Project Setup

1. Create ASP.NET Core Web API solution.
2. Create React + TypeScript frontend.
3. Add PostgreSQL connection.
4. Add authentication and tenant model.
5. Add basic chat page.

## Phase 2 - Tenant Configuration

1. Create tenant registration.
2. Add Swagger upload/register screen.
3. Parse Swagger JSON.
4. Store endpoints in PostgreSQL.
5. Generate embeddings for endpoint summary/description/path.

## Phase 3 - Agent MVP

Implement these agents first:

1. MasterOrchestratorAgent
2. NluIntentAgent
3. TenantContextAgent
4. SecurityRbacAgent
5. SkillRouterAgent
6. EndpointDiscoveryAgent
7. ApiExecutionAgent
8. ResponseComposerAgent
9. AuditTelemetryAgent

## Phase 4 - Chat Execution

1. User asks a question.
2. NLU extracts intent.
3. Skill router selects global skill.
4. Endpoint discovery finds tenant endpoint.
5. API execution calls tenant API.
6. Response composer formats result.
7. Frontend renders card/table/text.

## Phase 5 - MCP

1. Add MCP server registration.
2. Read MCP tools.
3. Store tool metadata.
4. Add MCP discovery agent.
5. Add MCP execution agent.
6. Add approval for high-risk MCP actions.

## Phase 6 - Database Assistant

1. Register tenant database.
2. Read schema only.
3. Store schema metadata.
4. Add DB schema search.
5. Add SQL generation.
6. Add SQL validation.
7. Start with read-only queries only.

## Phase 7 - Production Hardening

1. Add audit log.
2. Add token/cost tracking.
3. Add permission rules.
4. Add tenant isolation.
5. Add rate limiting.
6. Add retry and error recovery.
7. Add admin dashboard.

