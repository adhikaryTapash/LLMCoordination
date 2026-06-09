# Plan 02 — Database & EF Core Entities

## Goal

Add all PostgreSQL-ready EF Core entities from [docs/04-DATABASE-SCHEMA.md](../docs/04-DATABASE-SCHEMA.md), wire `AppDbContext`, and produce a **zero-error build** with a working migration.

## Prerequisites

- [Plan 01](01-solution-foundation.md) completed and building.

## Out of Scope

- Real PostgreSQL connection in production (use InMemory or SQLite for local zero-config build)
- JWT auth
- Agent implementation
- Embedding / pgvector (add column as `float[]` or skip vector until Plan 06)

---

## Step 1 — Add NuGet Packages

### Infrastructure project

```powershell
dotnet add src/LLMCoordination.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/LLMCoordination.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/LLMCoordination.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
```

### Api project (design-time)

```powershell
dotnet add src/LLMCoordination.Api package Microsoft.EntityFrameworkCore.Design
```

### Tests project (InMemory for unit tests)

```powershell
dotnet add tests/LLMCoordination.Tests package Microsoft.EntityFrameworkCore.InMemory
```

---

## Step 2 — Create Domain Entities

Implement entities in `src/LLMCoordination.Domain/Entities/` matching the schema doc.

### Multi-Tenant Tables

| Entity | Key Fields | Has tenant_id |
|--------|------------|---------------|
| `Tenant` | Id, Name, DomainType, Status, CreatedAt | — |
| `AppUser` | Id, TenantId, FullName, Email, PasswordHash, Role, Status | Yes |
| `TenantSwaggerDocument` | Id, TenantId, Name, DocumentUrl, RawJson, Version, Status | Yes |
| `TenantSwaggerEndpoint` | Id, TenantId, SwaggerDocumentId, Method, Path, OperationId, Summary, Description, RequestSchemaJson, ResponseSchemaJson, AuthRequired, Tags | Yes |
| `TenantMcpServer` | Id, TenantId, Name, BaseUrl, EncryptedCredentials, Status | Yes |
| `TenantMcpTool` | Id, TenantId, McpServerId, ToolName, Description, InputSchemaJson, OutputSchemaJson | Yes |
| `TenantDatabaseConnection` | Id, TenantId, DbType, Host, DatabaseName, EncryptedConnectionString, ReadOnly, Status | Yes |
| `TenantDatabaseSchema` | Id, TenantId, ConnectionId, TableName, ColumnName, DataType, IsNullable, RelationshipInfo | Yes |

### Global Skill Tables (no tenant_id)

| Entity | Key Fields |
|--------|------------|
| `GlobalSkill` | Id, SkillName, Domain, Category, Description, ActionType, Enabled |
| `GlobalSkillIntentExample` | Id, SkillId, ExampleText |
| `GlobalSkillPrompt` | Id, SkillId, PromptTemplate, Version, Active |
| `GlobalSkillResponseSchema` | Id, SkillId, ResponseSchemaJson, Version, Active |

### Mapping & Permission Tables

| Entity | Key Fields |
|--------|------------|
| `TenantSkillEndpointMapping` | Id, TenantId, SkillId, EndpointId, ConfidenceScore, Enabled |
| `TenantSkillMcpMapping` | Id, TenantId, SkillId, McpToolId, ConfidenceScore, Enabled |
| `TenantPermissionRule` | Id, TenantId, Role, Resource, Action, Allowed |

### Chat & Audit Tables

| Entity | Key Fields |
|--------|------------|
| `Conversation` | Id, TenantId, UserId, Title, CreatedAt |
| `ConversationMessage` | Id, ConversationId, Role, MessageText, ResponseJson, CreatedAt |
| `AgentAuditLog` | Id, TenantId, UserId, ConversationId, TurnId, AgentName, SkillName, ToolType, ToolName, RequestJson, ResponseJson, Status, Cost, ExecutionMs |

### Enums (`Domain/Enums/`)

- `DomainType`: Healthcare, Erp, Mixed
- `ToolType`: Swagger, Mcp, Database
- `AgentStatus`: Success, Failed, Skipped
- `SkillCategory`: Api, Database, Mcp, Analytics, Response

Use `Guid` for all primary keys (consistent, EF-friendly).

---

## Step 3 — Embedding Columns (Deferred Safe Approach)

For Plan 02, **do not** require pgvector extension. Options:

- **Option A:** Omit embedding columns; add in Plan 06.
- **Option B:** Add nullable `string? EmbeddingJson` placeholder column.

This keeps `dotnet build` and migrations working without PostgreSQL extensions.

---

## Step 4 — Create AppDbContext

`src/LLMCoordination.Infrastructure/Data/AppDbContext.cs`:

- DbSet for every entity above
- Fluent API configuration:
  - Required fields, max lengths
  - Indexes on `TenantId` for tenant-scoped tables
  - Foreign keys with restrict delete
  - Unique index on `GlobalSkill.SkillName`

---

## Step 5 — Register DbContext

`Infrastructure/DependencyInjection.cs`:

```csharp
services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        // Fallback for zero-config local build
        options.UseInMemoryDatabase("LLMCoordination_Dev");
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});
```

`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

Empty connection string → InMemory (builds without PostgreSQL installed).

---

## Step 6 — Create Initial Migration

Only if PostgreSQL is available OR use InMemory (skip migration file for InMemory):

```powershell
dotnet ef migrations add InitialCreate --project src/LLMCoordination.Infrastructure --startup-project src/LLMCoordination.Api
```

If EF tools not installed:

```powershell
dotnet tool install --global dotnet-ef
```

For InMemory-only dev: migration is optional; add a `DbInitializer` that ensures database is created on startup.

---

## Step 7 — Add DbContext Test

`tests/LLMCoordination.Tests/Data/AppDbContextTests.cs`:

```csharp
[Fact]
public void CanCreateDatabaseWithAllEntities()
{
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    using var ctx = new AppDbContext(options);
    ctx.Database.EnsureCreated();
    Assert.True(ctx.Tenants != null);
}
```

---

## Step 8 — Extend Health Check

Update `HealthController` to inject `AppDbContext` and verify `CanConnect()` or `Database.EnsureCreated()` succeeds.

Response adds: `"database": "connected"`.

---

## Deliverables Checklist

- [ ] All 18+ entity classes exist
- [ ] All enums exist
- [ ] `AppDbContext` with fluent configuration
- [ ] DI registration with InMemory fallback
- [ ] At least 1 passing EF Core test
- [ ] Health endpoint reports database status

---

## Build Verification (Must Pass)

```powershell
dotnet build
dotnet test
dotnet run --project src/LLMCoordination.Api
curl http://localhost:5xxx/api/health
```

### Expected Results

| Check | Expected |
|-------|----------|
| Build | 0 errors |
| Tests | All pass |
| Health | `"database": "connected"` |

---

## Common Build Errors & Fixes

| Error | Fix |
|-------|-----|
| Circular reference Domain ↔ Infrastructure | Entities stay in Domain only; DbContext in Infrastructure |
| Migration fails without PostgreSQL | Use InMemory fallback; skip Npgsql migration |
| Duplicate entity names | Use singular class names, plural DbSet names |
| Missing `DesignTimeDbContextFactory` | Add factory in Infrastructure for EF CLI |

---

## Next Plan

Proceed to [03-authentication-and-tenants.md](03-authentication-and-tenants.md).
