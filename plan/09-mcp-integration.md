# Plan 09 — MCP Integration

## Goal

Register MCP servers, discover and store tool metadata, route chat to MCP tools via global skills, and execute tools with approval gates — **zero build errors**, MCP calls can use a mock server in tests.

## Prerequisites

- [Plan 07](07-skill-registry-and-mappings.md) completed.

## Out of Scope

- Real production MCP over SSE (implement HTTP JSON-RPC stub first)
- OpenAI tool selection
- Frontend MCP pages beyond basic list (optional thin UI)

---

## Step 1 — Credential Encryption Service

`Infrastructure/Security/CredentialEncryptionService.cs`:

- Dev: AES with key from `appsettings` (`Encryption:DevKey`)
- Methods: `Encrypt(plainText)`, `Decrypt(cipherText)`
- Never log decrypted values

Register as singleton in DI.

---

## Step 2 — MCP Contracts

`Application/Contracts/`:

| Type | Fields |
|------|--------|
| `RegisterMcpServerRequest` | Name, BaseUrl, CredentialType, CredentialValue |
| `McpServerDto` | Id, Name, BaseUrl, Status, ToolCount |
| `McpToolDto` | Id, ToolName, Description, InputSchemaJson |
| `McpToolCallRequest` | ToolName, Arguments (Dictionary) |
| `McpToolCallResult` | Status, OutputJson, ErrorMessage |

---

## Step 3 — MCP Metadata Service

`Application/Services/McpMetadataService.cs`:

- `RegisterServerAsync` — encrypt credentials, save `TenantMcpServer`
- `DiscoverToolsAsync` — call MCP `tools/list` (or mock), save `TenantMcpTool`
- `GetToolsForTenantAsync` — list tools filtered by TenantId

`Infrastructure/Mcp/McpClient.cs`:

- HTTP client to MCP server base URL
- Dev fallback: if URL contains `mock`, return hardcoded tools from `MockMcpToolCatalog`

---

## Step 4 — MCP Agents

Implement (replace stubs if created in Plan 04):

| Agent | Responsibility |
|-------|----------------|
| `McpToolDiscoveryAgent` | Find tool by keyword match on name/description |
| `McpExecutionAgent` | Validate params, check approval, call McpClient |
| `ApprovalAgent` | Return `requiresApproval=true` for execute skills with actionType Create/Update/Delete |

Wire into orchestrator when `RoutingHints.RequiresMcpTool = true`.

---

## Step 5 — Tenant Skill MCP Mapping

Extend `TenantSkillMappingService`:

- On tool discovery, map tools to skills:
  - Read-like tools → `mcp.tool.execute`
  - Discovery queries → `mcp.tool.discovery`

Store in `TenantSkillMcpMapping`.

---

## Step 6 — McpController

`Api/Controllers/McpController.cs` — `[Authorize]`:

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/mcp/register` | Register server |
| POST | `/api/mcp/servers/{id}/discover` | Discover tools |
| GET | `/api/mcp/servers` | List servers |
| GET | `/api/mcp/tools` | List tools |
| POST | `/api/mcp/tools/{id}/test` | Test tool call (admin only) |

---

## Step 7 — Mock MCP for Tests

`tests/LLMCoordination.Tests/Mcp/MockMcpHandler.cs`:

- Inline test server or mock HttpMessageHandler
- Returns tools: `get_patient_summary`, `schedule_appointment`

Tests must not depend on external MCP infrastructure.

---

## Step 8 — Tests

| Test | Assert |
|------|--------|
| `RegisterMcp_EncryptsCredentials` | DB has encrypted blob, not plain |
| `DiscoverTools_StoresMetadata` | Tools linked to server |
| `McpExecution_RequiresApprovalForWrite` | Blocked without approval flag |
| `TenantIsolation_McpTools` | Tenant A cannot list Tenant B tools |

---

## Deliverables Checklist

- [ ] CredentialEncryptionService
- [ ] MCP register/discover/list APIs
- [ ] McpClient with mock fallback
- [ ] McpToolDiscoveryAgent + McpExecutionAgent
- [ ] TenantSkillMcpMapping auto-created
- [ ] All tests pass

---

## Build Verification (Must Pass)

```powershell
dotnet build
dotnet test
dotnet run --project src/LLMCoordination.Api

curl -X POST http://localhost:5xxx/api/mcp/register -H "Authorization: Bearer TOKEN" -H "Content-Type: application/json" -d "{\"name\":\"Mock MCP\",\"baseUrl\":\"mock://local\",\"credentialType\":\"none\",\"credentialValue\":\"\"}"

curl -X POST http://localhost:5xxx/api/mcp/servers/{id}/discover -H "Authorization: Bearer TOKEN"
curl http://localhost:5xxx/api/mcp/tools -H "Authorization: Bearer TOKEN"
```

### Expected Results

| Check | Expected |
|-------|----------|
| Build | 0 errors |
| Tests | All pass |
| Discover | ≥ 1 tool stored |
| Credentials | Encrypted in DB |

---

## Common Build Errors & Fixes

| Error | Fix |
|-------|-----|
| HttpClient base address invalid | Support `mock://` scheme with in-process handler |
| Decryption fails | Use same key for encrypt/decrypt in dev |
| Orchestrator doesn't route to MCP | Set RequiresMcpTool in NLU stub for test phrases |

---

## Next Plan

Proceed to [11-production-hardening.md](11-production-hardening.md) after Plan 10, or run Plan 10 in parallel.
