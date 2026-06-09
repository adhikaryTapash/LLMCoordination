# Frontend Folder Structure - React + TypeScript

```text
src/
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
      TenantDashboard.tsx
      TenantSettingsPage.tsx

    swagger/
      SwaggerUploadPage.tsx
      SwaggerEndpointList.tsx
      swaggerApi.ts
      swaggerTypes.ts

    mcp/
      McpServerPage.tsx
      McpToolList.tsx
      mcpApi.ts
      mcpTypes.ts

    skills/
      SkillRegistryPage.tsx
      SkillDetailPage.tsx
      SkillTestPage.tsx
      skillApi.ts
      skillTypes.ts

    audit/
      AuditLogPage.tsx
      auditApi.ts

  shared/
    components/
      Button.tsx
      Card.tsx
      Modal.tsx
      DataTable.tsx
      Loading.tsx
    api/
      httpClient.ts
    types/
      common.ts
    utils/
      formatters.ts
```

## Frontend Rendering Rule

The Chat API should return a `viewType`.

```text
viewType = card   => CardResultView
viewType = table  => TableResultView
viewType = chart  => ChartResultView
viewType = text   => Normal chat message
```

