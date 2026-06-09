# Architecture

## High-Level Flow

```text
User Chat
   |
   v
React Chat UI
   |
   v
ASP.NET Core Chat API
   |
   v
Master Orchestrator Agent
   |
   +--> NLU / Intent Agent
   +--> Tenant Context Agent
   +--> Security & RBAC Agent
   +--> Skill Router Agent
   +--> Query Planner Agent
   +--> Endpoint / MCP / DB Discovery Agent
   +--> Execution Agent
   +--> Response Composer Agent
   +--> Audit & Telemetry Agent
   |
   v
Chat Response
```

## Tenant Setup Flow

```text
Tenant Admin
   |
   | Upload Swagger / Register MCP / Register DB
   v
Configuration API
   |
   v
Parser Layer
   |
   +--> Swagger Parser
   +--> MCP Tool Reader
   +--> DB Schema Reader
   |
   v
Metadata Store + Vector Store
   |
   v
Available for Chat
```

## Runtime Question Flow

```text
User Question
   |
   v
NLU Agent
   |
   v
Skill Router
   |
   v
Tenant Tool Resolver
   |
   +--> Swagger Endpoint Catalog
   +--> MCP Tool Catalog
   +--> Database Schema Catalog
   |
   v
Execution Agent
   |
   v
Response Composer
```

## Example

User asks:

```text
Give me the list of patients in card view
```

System resolves:

```text
Intent: patient.list
Global Skill: api.query.execute
Tenant Endpoint: GET /api/patients
Response View: card
```

