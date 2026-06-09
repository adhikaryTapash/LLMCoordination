# Plan 10 — Database Assistant

## Goal

Register tenant database connections, introspect schema metadata, generate and validate read-only SQL, and answer database questions safely — **zero build errors**, no write SQL allowed.

## Prerequisites

- [Plan 07](07-skill-registry-and-mappings.md) completed.

## Out of Scope

- OpenAI SQL generation (use rule-based template SQL in this plan)
- pgvector on schema columns
- INSERT/UPDATE/DELETE SQL
- Direct production DB connections in CI tests (use SQLite/InMemory mock schema)

---

## Step 1 — Database Contracts

`Application/Contracts/`:

| Type | Fields |
|------|--------|
| `RegisterDatabaseRequest` | DbType, Host, DatabaseName, ConnectionString, ReadOnly |
| `DatabaseConnectionDto` | Id, DbType, Host, DatabaseName, ReadOnly, Status |
| `SchemaTableDto` | TableName, Columns[] |
| `SqlGenerationRequest` | Question, TenantId |
| `SqlGenerationResult` | Sql, Explanation, IsSafe |
| `SqlExecutionResult` | Columns[], Rows[], RowCount |

---

## Step 2 — Credential Storage

Reuse `CredentialEncryptionService` from Plan 09 (or implement here if Plan 09 skipped):

- Encrypt full connection string in `TenantDatabaseConnection.EncryptedConnectionString`
- Force `ReadOnly = true` in this plan

---

## Step 3 — DbSchemaReaderService

`Application/Services/DbSchemaReaderService.cs`:

- Connect read-only
- Read information_schema / pragma for SQLite
- Insert rows into `TenantDatabaseSchema` (one row per column)

Dev/test mode: accept `DbType=Mock` and load schema from `MockHealthcareSchema.json`.

---

## Step 4 — SQL Generation Agent (Rule-Based)

`Application/Agents/SqlGenerationAgent.cs`:

Keyword rules (no LLM):

```text
"count patients"     → SELECT COUNT(*) FROM patients
"list patients"      → SELECT * FROM patients LIMIT 20
"appointments today" → SELECT * FROM appointments WHERE date = CURRENT_DATE LIMIT 20
```

Return SQL + explanation. Unknown question → clarification request.

---

## Step 5 — SQL Validation Agent

`Application/Agents/SqlValidationAgent.cs`:

**Block** if SQL contains (case-insensitive):

- INSERT, UPDATE, DELETE, DROP, ALTER, TRUNCATE, CREATE, GRANT, REVoke

**Require:**

- SELECT only
- LIMIT clause (append `LIMIT 100` if missing)
- No multi-statement (`;` more than once)

Return `IsSafe=true/false` with reason.

---

## Step 6 — Database Execution Agent

`Application/Agents/DatabaseExecutionAgent.cs`:

- Only run if `SqlValidationAgent` passed
- Execute against tenant connection (or Mock provider in dev)
- Return tabular result for ResponseComposerAgent (table view)

---

## Step 7 — Database Schema Agent

`Application/Agents/DatabaseSchemaAgent.cs`:

- Search `TenantDatabaseSchema` by keyword
- Used for "what tables do I have?" questions

---

## Step 8 — DatabaseController

`Api/Controllers/DatabaseController.cs` — `[Authorize]`:

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/database/register` | Register connection |
| POST | `/api/database/connections/{id}/introspect` | Read schema |
| GET | `/api/database/connections` | List connections |
| GET | `/api/database/schema` | List schema metadata |
| POST | `/api/database/test-query` | Admin: run validated SQL only |

---

## Step 9 — Orchestrator Integration

Update `QueryPlannerAgent` (add if missing):

- If message mentions "database", "table", "SQL" → route to DB path
- Sequence: DatabaseSchemaAgent → SqlGenerationAgent → SqlValidationAgent → DatabaseExecutionAgent → ResponseComposerAgent

NLU stub: set `RequiresDatabase=true` for test phrases.

---

## Step 10 — Tests

| Test | Assert |
|------|--------|
| `SqlValidation_BlocksDelete` | IsSafe=false |
| `SqlValidation_AllowsSelectWithLimit` | IsSafe=true |
| `SqlGeneration_PatientList` | SQL contains SELECT + patients |
| `SchemaIntrospect_StoresColumns` | Rows in TenantDatabaseSchema |
| `Execution_MockDb_ReturnsRows` | RowCount > 0 |

---

## Deliverables Checklist

- [ ] Register/introspect/list database APIs
- [ ] Schema stored per tenant
- [ ] SQL generation (rule-based)
- [ ] SQL validation blocks unsafe queries
- [ ] Read-only execution
- [ ] Orchestrator routes DB questions
- [ ] All tests pass

---

## Build Verification (Must Pass)

```powershell
dotnet build
dotnet test
dotnet run --project src/LLMCoordination.Api

curl -X POST http://localhost:5xxx/api/database/register -H "Authorization: Bearer TOKEN" -H "Content-Type: application/json" -d "{\"dbType\":\"Mock\",\"host\":\"local\",\"databaseName\":\"healthcare\",\"connectionString\":\"mock\",\"readOnly\":true}"

curl -X POST http://localhost:5xxx/api/database/connections/{id}/introspect -H "Authorization: Bearer TOKEN"

curl -X POST http://localhost:5xxx/api/chat/message -H "Authorization: Bearer TOKEN" -H "Content-Type: application/json" -d "{\"message\":\"How many patients are in the database?\"}"
```

### Expected Results

| Check | Expected |
|-------|----------|
| Build | 0 errors |
| Tests | All pass |
| Unsafe SQL test endpoint | Rejected |
| Chat DB question | table view or count answer |

---

## Common Build Errors & Fixes

| Error | Fix |
|-------|-----|
| Connection string parse error | Use Mock db type in dev |
| Validation too aggressive | Allow subqueries in SELECT |
| Missing LIMIT | Auto-append in validator |

---

## Next Plan

Proceed to [11-production-hardening.md](11-production-hardening.md).
