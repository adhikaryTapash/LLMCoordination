# Plan 05 — Chat API MVP

## Goal

Expose `POST /api/chat/message` that runs the mock agent pipeline end-to-end and returns the structured response from [docs/05-API-CONTRACTS.md](../docs/05-API-CONTRACTS.md). Must compile, run, and return valid JSON with **zero errors**.

## Prerequisites

- [Plan 04](04-agent-contracts-and-services.md) completed.

## Out of Scope

- Real OpenAI NLU
- Real tenant API calls
- Conversation persistence (optional lightweight add — see Step 5)
- Frontend (Plan 08)

---

## Step 1 — ChatController

`Api/Controllers/ChatController.cs`:

```csharp
[Authorize]
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    [HttpPost("message")]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
}
```

Flow:

1. Build `AgentContext` from JWT (TenantId, UserId, Role) + request
2. Call `ChatOrchestrationService.ExecuteAsync`
3. Return `ChatResponse`

**Never** read `tenantId` from request body.

---

## Step 2 — Request Validation

- `Message` required, max 4000 chars
- `ConversationId` optional — generate `Guid` if null/empty
- `ViewPreference` optional — enum: card, table, chart, text

Return `400 Bad Request` with problem details for invalid input.

---

## Step 3 — Response Shape

Ensure response matches contract:

```json
{
  "conversationId": "...",
  "messageId": "...",
  "intent": "patient.list",
  "skill": "api.query.execute",
  "viewType": "card",
  "status": "success",
  "answer": "Here is the list of patients.",
  "data": { "totalRecords": 3, "cards": [...] }
}
```

Use consistent JSON property naming (camelCase).

---

## Step 4 — CORS (Prepare for Frontend)

In `Program.cs` (Development only):

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
app.UseCors("DevFrontend");
```

---

## Step 5 — Optional Conversation Persistence

If adding persistence in this plan (recommended, low risk):

- Create/find `Conversation` by Id + TenantId
- Append `ConversationMessage` for user message and assistant response
- Store `ResponseJson` on assistant message

If skipped, document that persistence comes in Plan 11 — **still builds fine**.

---

## Step 6 — Integration Tests

`tests/LLMCoordination.Tests/Chat/ChatControllerTests.cs`:

| Test | Assert |
|------|--------|
| `SendMessage_WithoutAuth_Returns401` | Unauthorized |
| `SendMessage_PatientList_ReturnsCardView` | viewType=card, cards present |
| `SendMessage_EmptyMessage_Returns400` | Bad request |

Use `WebApplicationFactory` + InMemory DB + test JWT or bypass auth in test config.

---

## Step 7 — HTTP Test File

Update or create `LLMCoordination.http`:

```http
### Login
POST {{baseUrl}}/api/auth/login
Content-Type: application/json

{ "email": "admin@demo.local", "password": "Demo123!" }

### Chat - Patient List
POST {{baseUrl}}/api/chat/message
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "message": "Give me the list of patients in card view",
  "viewPreference": "card"
}
```

---

## Deliverables Checklist

- [ ] `POST /api/chat/message` implemented
- [ ] JWT required
- [ ] Mock pipeline returns full ChatResponse
- [ ] Request validation
- [ ] CORS for localhost:5173
- [ ] Integration tests pass
- [ ] `.http` file for manual testing

---

## Build Verification (Must Pass)

```powershell
dotnet build
dotnet test
dotnet run --project src/LLMCoordination.Api

# Login, then chat
curl -X POST http://localhost:5xxx/api/chat/message ^
  -H "Authorization: Bearer TOKEN" ^
  -H "Content-Type: application/json" ^
  -d "{\"message\":\"Give me the list of patients in card view\",\"viewPreference\":\"card\"}"
```

### Expected Results

| Check | Expected |
|-------|----------|
| Build | 0 errors |
| Tests | All pass |
| Chat response | status=success, viewType=card, data.cards.length=3 |

---

## Common Build Errors & Fixes

| Error | Fix |
|-------|-----|
| 401 on chat | Include valid Bearer token |
| Empty data object | Verify ResponseComposerAgent stub output |
| CORS error from frontend | Enable DevFrontend policy before UseAuthorization |

---

## Next Plans

- Backend: [06-swagger-parser-and-registration.md](06-swagger-parser-and-registration.md)
- Frontend (can start now): [08-frontend-foundation-and-chat-ui.md](08-frontend-foundation-and-chat-ui.md)
