using LLMCoordination.Application.Agents;
using LLMCoordination.Application.Contracts;

namespace LLMCoordination.Application.Services;

public class ChatOrchestrationService
{
    private readonly NluIntentAgent _nluIntentAgent;
    private readonly ClarificationAgent _clarificationAgent;
    private readonly TenantContextAgent _tenantContextAgent;
    private readonly SecurityRbacAgent _securityRbacAgent;
    private readonly ApprovalAgent _approvalAgent;
    private readonly QueryPlannerAgent _queryPlannerAgent;
    private readonly SkillRouterAgent _skillRouterAgent;
    private readonly EndpointDiscoveryAgent _endpointDiscoveryAgent;
    private readonly ApiExecutionAgent _apiExecutionAgent;
    private readonly McpToolDiscoveryAgent _mcpToolDiscoveryAgent;
    private readonly McpExecutionAgent _mcpExecutionAgent;
    private readonly DatabaseSchemaAgent _databaseSchemaAgent;
    private readonly SqlGenerationAgent _sqlGenerationAgent;
    private readonly SqlValidationAgent _sqlValidationAgent;
    private readonly DatabaseExecutionAgent _databaseExecutionAgent;
    private readonly AnalyticsAgent _analyticsAgent;
    private readonly ResponseComposerAgent _responseComposerAgent;
    private readonly AuditTelemetryAgent _auditTelemetryAgent;
    private readonly ErrorRecoveryAgent _errorRecoveryAgent;

    public ChatOrchestrationService(
        NluIntentAgent nluIntentAgent,
        ClarificationAgent clarificationAgent,
        TenantContextAgent tenantContextAgent,
        SecurityRbacAgent securityRbacAgent,
        ApprovalAgent approvalAgent,
        QueryPlannerAgent queryPlannerAgent,
        SkillRouterAgent skillRouterAgent,
        EndpointDiscoveryAgent endpointDiscoveryAgent,
        ApiExecutionAgent apiExecutionAgent,
        McpToolDiscoveryAgent mcpToolDiscoveryAgent,
        McpExecutionAgent mcpExecutionAgent,
        DatabaseSchemaAgent databaseSchemaAgent,
        SqlGenerationAgent sqlGenerationAgent,
        SqlValidationAgent sqlValidationAgent,
        DatabaseExecutionAgent databaseExecutionAgent,
        AnalyticsAgent analyticsAgent,
        ResponseComposerAgent responseComposerAgent,
        AuditTelemetryAgent auditTelemetryAgent,
        ErrorRecoveryAgent errorRecoveryAgent)
    {
        _nluIntentAgent = nluIntentAgent;
        _clarificationAgent = clarificationAgent;
        _tenantContextAgent = tenantContextAgent;
        _securityRbacAgent = securityRbacAgent;
        _approvalAgent = approvalAgent;
        _queryPlannerAgent = queryPlannerAgent;
        _skillRouterAgent = skillRouterAgent;
        _endpointDiscoveryAgent = endpointDiscoveryAgent;
        _apiExecutionAgent = apiExecutionAgent;
        _mcpToolDiscoveryAgent = mcpToolDiscoveryAgent;
        _mcpExecutionAgent = mcpExecutionAgent;
        _databaseSchemaAgent = databaseSchemaAgent;
        _sqlGenerationAgent = sqlGenerationAgent;
        _sqlValidationAgent = sqlValidationAgent;
        _databaseExecutionAgent = databaseExecutionAgent;
        _analyticsAgent = analyticsAgent;
        _responseComposerAgent = responseComposerAgent;
        _auditTelemetryAgent = auditTelemetryAgent;
        _errorRecoveryAgent = errorRecoveryAgent;
    }

    public async Task<ChatResponse> ExecuteAsync(
        ChatRequest request,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        context.RawMessage = request.Message;
        if (!string.IsNullOrWhiteSpace(request.ViewPreference))
        {
            context.ViewPreference = request.ViewPreference;
        }

        await RunAgentAsync(_nluIntentAgent, context, cancellationToken);

        var clarificationResult = await RunAgentAsync(_clarificationAgent, context, cancellationToken);
        if (clarificationResult.Status == Domain.Enums.AgentStatus.Failed)
        {
            return BuildClarificationResponse(context, clarificationResult);
        }

        var tenantResult = await RunAgentAsync(_tenantContextAgent, context, cancellationToken);
        if (tenantResult.Status == Domain.Enums.AgentStatus.Failed)
        {
            return BuildErrorResponse(context, tenantResult.ErrorMessage ?? "Tenant context failed.");
        }

        var rbacResult = await RunAgentAsync(_securityRbacAgent, context, cancellationToken);
        if (rbacResult.Status == Domain.Enums.AgentStatus.Failed)
        {
            await RunAgentAsync(_errorRecoveryAgent, context, cancellationToken);
            return BuildErrorResponse(context, rbacResult.ErrorMessage ?? "Access denied.");
        }

        var approvalResult = await RunAgentAsync(_approvalAgent, context, cancellationToken);
        if (approvalResult.Status == Domain.Enums.AgentStatus.Failed)
        {
            return BuildApprovalResponse(context, approvalResult);
        }

        await RunAgentAsync(_queryPlannerAgent, context, cancellationToken);
        await RunAgentAsync(_skillRouterAgent, context, cancellationToken);

        var nlu = GetNluResponse(context);
        if (nlu?.RoutingHints.RequiresDatabase == true)
        {
            await RunDatabasePipelineAsync(context, cancellationToken);
        }
        else if (nlu?.RoutingHints.RequiresMcpTool == true)
        {
            await RunAgentAsync(_mcpToolDiscoveryAgent, context, cancellationToken);
            await RunAgentAsync(_mcpExecutionAgent, context, cancellationToken);
        }
        else if (nlu?.Intent.Name.Contains("analytics", StringComparison.OrdinalIgnoreCase) == true)
        {
            await RunAgentAsync(_analyticsAgent, context, cancellationToken);
        }
        else
        {
            await RunAgentAsync(_endpointDiscoveryAgent, context, cancellationToken);
            await RunAgentAsync(_apiExecutionAgent, context, cancellationToken);
        }

        await RunAgentAsync(_responseComposerAgent, context, cancellationToken);
        await RunAgentAsync(_auditTelemetryAgent, context, cancellationToken);

        return BuildSuccessResponse(context);
    }

    private async Task RunDatabasePipelineAsync(AgentContext context, CancellationToken cancellationToken)
    {
        await RunAgentAsync(_databaseSchemaAgent, context, cancellationToken);
        await RunAgentAsync(_sqlGenerationAgent, context, cancellationToken);

        var validationResult = await RunAgentAsync(_sqlValidationAgent, context, cancellationToken);
        if (validationResult.Status == Domain.Enums.AgentStatus.Failed)
        {
            await RunAgentAsync(_errorRecoveryAgent, context, cancellationToken);
            return;
        }

        await RunAgentAsync(_databaseExecutionAgent, context, cancellationToken);
    }

    private static async Task<AgentResult> RunAgentAsync(
        IAgent agent,
        AgentContext context,
        CancellationToken cancellationToken)
    {
        return await agent.ExecuteAsync(context, cancellationToken);
    }

    private static ChatResponse BuildSuccessResponse(AgentContext context)
    {
        var nlu = GetNluResponse(context);
        var skill = context.AgentOutputs.TryGetValue(SkillRouterAgent.AgentName, out var skillObj) &&
                    skillObj is SkillRouterResult skillResult
            ? skillResult.SelectedSkill
            : string.Empty;

        var viewType = !string.IsNullOrWhiteSpace(context.ViewPreference)
            ? context.ViewPreference
            : nlu?.Entities.ViewType ?? "text";

        context.AgentOutputs.TryGetValue(ResponseComposerAgent.AgentName, out var data);

        var answer = nlu?.Intent.Name switch
        {
            "patient.list" => "Here is the list of patients.",
            "appointment.list.upcoming" => "Here are the upcoming appointments.",
            "database.query.count" => "Here is the database query result.",
            _ => "Request processed successfully."
        };

        return new ChatResponse
        {
            ConversationId = context.ConversationId.ToString(),
            MessageId = Guid.NewGuid().ToString("N"),
            Intent = nlu?.Intent.Name ?? string.Empty,
            Skill = skill,
            ViewType = viewType,
            Status = "success",
            Answer = answer,
            Data = data
        };
    }

    private static ChatResponse BuildClarificationResponse(AgentContext context, AgentResult result)
    {
        return new ChatResponse
        {
            ConversationId = context.ConversationId.ToString(),
            MessageId = Guid.NewGuid().ToString("N"),
            Status = "clarification_required",
            Answer = result.ErrorMessage ?? "Additional information required.",
            Intent = GetNluResponse(context)?.Intent.Name ?? string.Empty
        };
    }

    private static ChatResponse BuildApprovalResponse(AgentContext context, AgentResult result)
    {
        return new ChatResponse
        {
            ConversationId = context.ConversationId.ToString(),
            MessageId = Guid.NewGuid().ToString("N"),
            Status = "approval_required",
            Answer = result.ErrorMessage ?? "Approval required.",
            Intent = GetNluResponse(context)?.Intent.Name ?? string.Empty
        };
    }

    private static ChatResponse BuildErrorResponse(AgentContext context, string message)
    {
        return new ChatResponse
        {
            ConversationId = context.ConversationId.ToString(),
            MessageId = Guid.NewGuid().ToString("N"),
            Status = "error",
            Answer = message,
            Intent = GetNluResponse(context)?.Intent.Name ?? string.Empty
        };
    }

    private static NluAgentResponse? GetNluResponse(AgentContext context) =>
        context.AgentOutputs.TryGetValue(NluIntentAgent.AgentName, out var output) && output is NluAgentResponse nlu
            ? nlu
            : null;
}
