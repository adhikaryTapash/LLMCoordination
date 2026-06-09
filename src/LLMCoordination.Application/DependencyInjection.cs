using LLMCoordination.Application.Agents;
using LLMCoordination.Application.Services;
using LLMCoordination.Application.Skills;
using Microsoft.Extensions.DependencyInjection;

namespace LLMCoordination.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<AuthService>();
        services.AddScoped<TenantContextService>();
        services.AddScoped<ChatOrchestrationService>();
        services.AddScoped<SwaggerParserService>();
        services.AddScoped<SwaggerRegistrationService>();
        services.AddScoped<McpMetadataService>();
        services.AddScoped<DbSchemaReaderService>();

        services.AddScoped<SkillRegistryService>();
        services.AddScoped<TenantSkillMappingService>();
        services.AddScoped<SkillExecutionService>();

        services.AddScoped<MasterOrchestratorAgent>();
        services.AddScoped<NluIntentAgent>();
        services.AddScoped<TenantContextAgent>();
        services.AddScoped<SecurityRbacAgent>();
        services.AddScoped<SkillRouterAgent>();
        services.AddScoped<EndpointDiscoveryAgent>();
        services.AddScoped<ApiExecutionAgent>();
        services.AddScoped<McpToolDiscoveryAgent>();
        services.AddScoped<McpExecutionAgent>();
        services.AddScoped<DatabaseSchemaAgent>();
        services.AddScoped<SqlGenerationAgent>();
        services.AddScoped<SqlValidationAgent>();
        services.AddScoped<DatabaseExecutionAgent>();
        services.AddScoped<QueryPlannerAgent>();
        services.AddScoped<ResponseComposerAgent>();
        services.AddScoped<ClarificationAgent>();
        services.AddScoped<ApprovalAgent>();
        services.AddScoped<ErrorRecoveryAgent>();
        services.AddScoped<AuditTelemetryAgent>();
        services.AddScoped<AnalyticsAgent>();

        return services;
    }
}
