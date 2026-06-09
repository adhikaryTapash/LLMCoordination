# Plan 08 — Frontend Foundation & Chat UI

## Goal

Create a React + TypeScript frontend that authenticates, sends chat messages to `POST /api/chat/message`, and renders card/table/text views — **`npm run build` with zero errors**.

## Prerequisites

- [Plan 05](05-chat-api-mvp.md) completed (Chat API + CORS).
- Plan 03 login endpoint available.

## Out of Scope

- Swagger/MCP/Skill admin pages (stub routes OK)
- Real OpenAI streaming
- Production deployment

---

## Step 1 — Create Vite React Project

From repo root:

```powershell
npm create vite@latest client -- --template react-ts
cd client
npm install
npm install axios zustand react-router-dom
```

---

## Step 2 — Folder Structure

Match [docs/07-FRONTEND-FOLDER-STRUCTURE.md](../docs/07-FRONTEND-FOLDER-STRUCTURE.md):

```text
client/src/
  app/
    App.tsx
    router.tsx
  features/
    auth/
      LoginPage.tsx
      authApi.ts
      authStore.ts
    chat/
      ChatPage.tsx
      ChatInput.tsx
      ChatMessage.tsx
      CardResultView.tsx
      TableResultView.tsx
      ChartResultView.tsx
      chatApi.ts
      chatTypes.ts
    tenant/
      TenantDashboard.tsx      (placeholder)
    swagger/
      SwaggerUploadPage.tsx    (placeholder)
    mcp/
      McpServerPage.tsx          (placeholder)
    skills/
      SkillRegistryPage.tsx    (placeholder)
    audit/
      AuditLogPage.tsx           (placeholder)
  shared/
    components/
      Button.tsx
      Card.tsx
      Loading.tsx
    api/
      httpClient.ts
    types/
      common.ts
```

Placeholder pages: simple `<h1>Coming soon</h1>` — must compile.

---

## Step 3 — HTTP Client

`shared/api/httpClient.ts`:

- Base URL from `import.meta.env.VITE_API_BASE_URL` (default `http://localhost:5xxx`)
- Attach `Authorization: Bearer {token}` from auth store
- Handle 401 → redirect to login

`.env.development`:

```text
VITE_API_BASE_URL=http://localhost:5000
```

Adjust port to match API launchSettings.

---

## Step 4 — Auth Feature

`authStore.ts` (Zustand):

- State: token, userId, tenantId, role, fullName
- Actions: login, logout, loadFromStorage

`LoginPage.tsx`:

- Email + password form
- Calls `POST /api/auth/login`
- Redirect to `/chat` on success

---

## Step 5 — Chat Types

`chatTypes.ts` — mirror backend contract:

```typescript
export interface ChatRequest {
  conversationId?: string;
  message: string;
  viewPreference?: 'card' | 'table' | 'chart' | 'text';
}

export interface ChatResponse {
  conversationId: string;
  messageId: string;
  intent: string;
  skill: string;
  viewType: 'card' | 'table' | 'chart' | 'text';
  status: string;
  answer: string;
  data?: CardData | TableData | unknown;
}
```

Define `CardData`, `CardItem`, `TableData` matching API samples.

---

## Step 6 — Chat UI Components

| Component | Behavior |
|-----------|----------|
| `ChatPage` | Message list + input; stores conversationId |
| `ChatInput` | Text box + send button |
| `ChatMessage` | User vs assistant bubble |
| `CardResultView` | Renders `data.cards[]` using shared `Card` |
| `TableResultView` | Renders columns + rows |
| `ChartResultView` | Placeholder: "Chart view not implemented" |

**Rendering rule** from docs:

```text
viewType = card  → CardResultView
viewType = table → TableResultView
viewType = chart → ChartResultView
viewType = text  → answer string only
```

---

## Step 7 — Router

`app/router.tsx`:

| Path | Component | Auth |
|------|-----------|------|
| `/login` | LoginPage | Public |
| `/chat` | ChatPage | Protected |
| `/` | Redirect to /chat | — |
| `/tenant`, `/swagger`, `/mcp`, `/skills`, `/audit` | Placeholders | Protected |

Protected route wrapper checks token in store.

---

## Step 8 — TypeScript Strict Mode

Ensure `tsconfig.json` has `"strict": true`. Fix all type errors — **no `any`** except at JSON boundaries with explicit casts.

---

## Step 9 — Build & Lint

Add scripts to `client/package.json`:

```json
{
  "scripts": {
    "build": "tsc -b && vite build",
    "lint": "eslint ."
  }
}
```

---

## Deliverables Checklist

- [ ] Vite React TS app in `client/`
- [ ] Login flow works against API
- [ ] Chat sends message and displays response
- [ ] Card/table/text rendering works
- [ ] Protected routes
- [ ] Placeholder pages compile
- [ ] `npm run build` succeeds

---

## Build Verification (Must Pass)

```powershell
# Terminal 1 — API
dotnet run --project src/LLMCoordination.Api

# Terminal 2 — Frontend
cd client
npm install
npm run build
npm run dev
```

Manual test:

1. Open `http://localhost:5173`
2. Login with dev credentials
3. Send: "Give me the list of patients in card view"
4. See card layout with 3 patients

### Expected Results

| Check | Expected |
|-------|----------|
| `npm run build` | 0 errors |
| `tsc -b` | 0 errors |
| Login | Redirects to chat |
| Chat | Cards rendered |

---

## Common Build Errors & Fixes

| Error | Fix |
|-------|-----|
| CORS blocked | Enable CORS in API (Plan 05) |
| TS error on data union | Narrow by viewType before render |
| 401 on chat | Token attached in httpClient interceptor |
| VITE_API_BASE_URL undefined | Add `.env.development` |

---

## Next Plan

Backend continues with [09-mcp-integration.md](09-mcp-integration.md) or [10-database-assistant.md](10-database-assistant.md).
