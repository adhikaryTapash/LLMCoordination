# VIBE AI SaaS Blueprint

## Goal
Build a multi-tenant AI web application where each tenant can register:

- Swagger/OpenAPI documentation
- MCP server credentials/tools
- Database connection/schema details

Then users can ask natural-language questions in chat, and the system uses Microsoft Agentic Framework style orchestration with OpenAI to answer using APIs, MCP tools, or database metadata.

## Technology Stack

- Backend: C# / ASP.NET Core Web API
- Database: PostgreSQL
- Vector Search: PostgreSQL pgvector
- Frontend: React + TypeScript
- AI: OpenAI / Azure OpenAI
- Agent Style: Microsoft Agentic Framework pattern
- Auth: JWT-based multi-tenant authentication
- Hosting: Azure App Service / Container App / VM

## Core Principle

Do not create one skill per API endpoint.

Use global reusable skills and map tenant-specific Swagger APIs, MCP tools, and database schema under those skills.

```text
Agent = workflow brain
Skill = reusable business capability
Tool = tenant-specific Swagger/MCP/DB operation
```

