# Cursor VIBE Coding Prompts

## Prompt 1 - Create Solution Structure

Create a full-stack SaaS application using C# ASP.NET Core Web API, PostgreSQL, and React with TypeScript.

The product is a multi-tenant AI assistant platform. Tenants can register Swagger/OpenAPI docs, MCP server details, and database schema details. Users ask questions in chat. The backend uses an agentic architecture to understand intent, route to a reusable global skill, discover the correct tenant endpoint/tool/database schema, execute safely, and return a structured response.

Create the backend and frontend folder structure exactly as described in the blueprint files.

## Prompt 2 - Create Database Entities

Create PostgreSQL-ready EF Core entities for tenants, users, global skills, skill intent examples, skill prompts, skill response schemas, tenant swagger documents, tenant swagger endpoints, tenant MCP servers, tenant MCP tools, tenant database connections, tenant database schema, conversations, messages, and audit logs.

Use tenant_id for tenant-specific entities. Do not add tenant_id to global skill master tables.

## Prompt 3 - Create Chat API

Create ChatController with POST /api/chat/message.

The request should accept conversationId and message. The response should return conversationId, messageId, intent, skill, viewType, status, answer, and data.

Do not implement real OpenAI call yet. Use mock NLU and mock endpoint execution first.

## Prompt 4 - Create Agent Contracts

Create AgentContext, AgentResult, IntentResult, SkillDefinition, ToolExecutionRequest, ToolExecutionResult, ChatRequest, ChatResponse, CardResponse, and TableResponse classes.

## Prompt 5 - Create MVP Agents

Create these agents as injectable services:

- MasterOrchestratorAgent
- NluIntentAgent
- TenantContextAgent
- SecurityRbacAgent
- SkillRouterAgent
- EndpointDiscoveryAgent
- ApiExecutionAgent
- ResponseComposerAgent
- AuditTelemetryAgent

Each agent should accept AgentContext and return AgentResult.

## Prompt 6 - Swagger Parser

Create a SwaggerParserService that accepts OpenAPI JSON and extracts method, path, operationId, summary, description, request schema, response schema, tags, and auth requirement. Store parsed endpoints in tenant_swagger_endpoints.

## Prompt 7 - Frontend Chat UI

Create React TypeScript ChatPage with input box and message list. When API response viewType is card, render CardResultView. When viewType is table, render TableResultView. Otherwise render text.

## Prompt 8 - Skill Registry

Create global reusable skills:

- api.query.execute
- api.record.create
- api.record.update
- api.record.delete
- swagger.endpoint.search
- swagger.endpoint.explain
- database.schema.search
- database.sql.generate
- mcp.tool.discovery
- mcp.tool.execute

Create seed data for these skills.

## Prompt 9 - Security Rules

Implement RBAC checks for read, create, update, delete. Block all delete operations unless approval flag is true. Add audit log for every chat execution.

## Prompt 10 - Production Hardening

Add error handling, request validation, rate limiting, logging, tenant isolation, encrypted credentials, and safe SQL validation.

