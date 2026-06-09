using System.Text.Json;
using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class SkillRouterAgent : IAgent
{
    public const string AgentName = "skill.router";

    private readonly ISkillRepository _skillRepository;

    public SkillRouterAgent(ISkillRepository skillRepository)
    {
        _skillRepository = skillRepository;
    }

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var nlu = GetNluResponse(context);
        var intentName = nlu?.Intent.Name ?? "general.inquiry";
        var suggestedSkill = nlu?.RoutingHints.SuggestedSkill;

        var skill = !string.IsNullOrWhiteSpace(suggestedSkill)
            ? await _skillRepository.GetByNameAsync(suggestedSkill, cancellationToken)
            : await _skillRepository.FindByIntentAsync(intentName, cancellationToken);

        var selectedSkill = skill?.SkillName ?? MapIntentToSkill(intentName);
        var result = new SkillRouterResult
        {
            SelectedSkill = selectedSkill,
            Reason = skill is not null ? "Matched from skill registry" : "Rule-based fallback mapping"
        };

        context.AgentOutputs[AgentName] = result;

        return new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(result),
            NextAgentHint = "endpoint.discovery"
        };
    }

    private static string MapIntentToSkill(string intentName)
    {
        if (intentName.StartsWith("patient.list", StringComparison.OrdinalIgnoreCase) ||
            intentName.StartsWith("appointment.list", StringComparison.OrdinalIgnoreCase))
        {
            return "api.query.execute";
        }

        if (intentName.Contains(".delete", StringComparison.OrdinalIgnoreCase))
        {
            return "api.record.delete";
        }

        if (intentName.Contains(".create", StringComparison.OrdinalIgnoreCase))
        {
            return "api.record.create";
        }

        if (intentName.Contains(".update", StringComparison.OrdinalIgnoreCase))
        {
            return "api.record.update";
        }

        if (intentName.StartsWith("database.", StringComparison.OrdinalIgnoreCase))
        {
            return "database.sql.generate";
        }

        return "api.query.execute";
    }

    private static NluAgentResponse? GetNluResponse(AgentContext context) =>
        context.AgentOutputs.TryGetValue(NluIntentAgent.AgentName, out var output) && output is NluAgentResponse nlu
            ? nlu
            : null;
}
