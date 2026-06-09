using System.Diagnostics;
using System.Text.Json;
using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Entities;
using LLMCoordination.Domain.Enums;
using ToolType = LLMCoordination.Domain.Enums.ToolType;
using Microsoft.Extensions.Logging;

namespace LLMCoordination.Application.Agents;

public class AuditTelemetryAgent : IAgent
{
    public const string AgentName = "audit.telemetry";

    private readonly IAuditRepository _auditRepository;
    private readonly ILogger<AuditTelemetryAgent> _logger;

    public AuditTelemetryAgent(IAuditRepository auditRepository, ILogger<AuditTelemetryAgent> logger)
    {
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var nlu = GetNluResponse(context);
        var skill = context.AgentOutputs.TryGetValue(SkillRouterAgent.AgentName, out var skillObj) &&
                    skillObj is SkillRouterResult skillResult
            ? skillResult.SelectedSkill
            : string.Empty;

        var log = new AgentAuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = context.TenantId,
            UserId = context.UserId,
            ConversationId = context.ConversationId,
            TurnId = Guid.TryParse(context.TurnId, out var turnId) ? turnId : Guid.NewGuid(),
            AgentName = AgentName,
            SkillName = skill,
            ToolType = nlu?.RoutingHints.RequiresDatabase == true ? ToolType.Database : ToolType.Swagger,
            ToolName = nlu?.Intent.Resource ?? "unknown",
            RequestJson = JsonSerializer.Serialize(new { context.RawMessage, context.ViewPreference }),
            ResponseJson = JsonSerializer.Serialize(context.AgentOutputs.Keys),
            Status = AgentStatus.Success,
            Cost = 0,
            ExecutionMs = (int)stopwatch.ElapsedMilliseconds,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _logger.LogInformation(
            "Audit: Tenant={TenantId} User={UserId} Intent={Intent} Skill={Skill} Agents={AgentCount}",
            context.TenantId,
            context.UserId,
            nlu?.Intent.Name,
            skill,
            context.AgentOutputs.Count);

        await _auditRepository.AddAsync(log, cancellationToken);

        context.AgentOutputs[AgentName] = new { logged = true, log.Id };

        return new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(new { logged = true })
        };
    }

    private static NluAgentResponse? GetNluResponse(AgentContext context) =>
        context.AgentOutputs.TryGetValue(NluIntentAgent.AgentName, out var output) && output is NluAgentResponse nlu
            ? nlu
            : null;
}
