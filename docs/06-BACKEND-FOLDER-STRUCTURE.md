# Backend Folder Structure - C# ASP.NET Core

```text
src/
  VibeAI.Api/
    Controllers/
      AuthController.cs
      ChatController.cs
      TenantController.cs
      SwaggerController.cs
      McpController.cs
      SkillController.cs
    Program.cs
    appsettings.json

  VibeAI.Application/
    Agents/
      MasterOrchestratorAgent.cs
      NluIntentAgent.cs
      TenantContextAgent.cs
      SecurityRbacAgent.cs
      SkillRouterAgent.cs
      EndpointDiscoveryAgent.cs
      McpToolDiscoveryAgent.cs
      DatabaseSchemaAgent.cs
      QueryPlannerAgent.cs
      ApiExecutionAgent.cs
      McpExecutionAgent.cs
      SqlGenerationAgent.cs
      SqlValidationAgent.cs
      DatabaseExecutionAgent.cs
      ResponseComposerAgent.cs
      ClarificationAgent.cs
      ApprovalAgent.cs
      AuditTelemetryAgent.cs
      ErrorRecoveryAgent.cs
      AnalyticsAgent.cs

    Contracts/
      AgentContext.cs
      AgentResult.cs
      IntentResult.cs
      SkillDefinition.cs
      ChatRequest.cs
      ChatResponse.cs
      CardResponse.cs
      TableResponse.cs

    Skills/
      SkillRegistryService.cs
      SkillExecutionService.cs
      SkillPromptBuilder.cs

    Services/
      ChatOrchestrationService.cs
      SwaggerParserService.cs
      McpMetadataService.cs
      DbSchemaReaderService.cs
      EmbeddingService.cs
      VectorSearchService.cs
      CredentialEncryptionService.cs

  VibeAI.Domain/
    Entities/
      Tenant.cs
      AppUser.cs
      GlobalSkill.cs
      TenantSwaggerEndpoint.cs
      TenantMcpTool.cs
      Conversation.cs
      AuditLog.cs
    Enums/
      DomainType.cs
      ToolType.cs
      AgentStatus.cs
      SkillCategory.cs

  VibeAI.Infrastructure/
    Data/
      AppDbContext.cs
      Migrations/
    Repositories/
    OpenAI/
    Swagger/
    Mcp/
    Security/
    Telemetry/

  VibeAI.Tests/
```

