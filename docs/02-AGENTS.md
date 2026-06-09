# Agent List

## 1. Master Orchestrator Agent
Controls full chat flow and decides which agent should run next.

## 2. NLU / Intent Agent
Understands user question, domain, action, entities, filters, view type, and confidence.

## 3. Tenant Context Agent
Loads tenant settings, registered Swagger documents, MCP servers, DB schemas, permissions, and user context.

## 4. Security & RBAC Agent
Checks whether the user can perform read, create, update, delete, export, or sensitive-data actions.

## 5. Skill Router Agent
Selects the correct global reusable skill from the skill registry.

## 6. Endpoint Discovery Agent
Finds the best matching Swagger/OpenAPI endpoint for the user question.

## 7. MCP Tool Discovery Agent
Finds matching MCP tools and their required parameters.

## 8. Database Schema Agent
Understands tenant tables, columns, relationships, and safe query boundaries.

## 9. Query Planner Agent
Decides whether the question should use API, MCP, database, RAG, or a combination.

## 10. API Execution Agent
Executes Swagger/OpenAPI endpoints safely.

## 11. MCP Execution Agent
Executes MCP tools after permission and approval checks.

## 12. SQL Generation Agent
Generates SQL from business questions.

## 13. SQL Validation Agent
Blocks unsafe SQL and enforces tenant filters and read-only rules.

## 14. Database Execution Agent
Executes safe database queries.

## 15. Response Composer Agent
Converts raw results into card, table, chart, JSON, summary, or text response.

## 16. Clarification Agent
Asks follow-up questions when patient/customer/product/API identity is ambiguous.

## 17. Approval / HITL Agent
Requires approval for create, update, delete, financial, or sensitive actions.

## 18. Audit & Telemetry Agent
Logs intent, selected skill, selected tool, cost, execution time, and response.

## 19. Error Recovery Agent
Handles missing parameters, API errors, timeout, invalid schema, and retry decisions.

## 20. Analytics Agent
Handles trend, prediction, comparison, business insight, and summary questions.

## Minimum MVP Agents

Start with:

1. Master Orchestrator Agent
2. NLU / Intent Agent
3. Tenant Context Agent
4. Security & RBAC Agent
5. Skill Router Agent
6. Endpoint Discovery Agent
7. API Execution Agent
8. Response Composer Agent
9. Audit Agent

