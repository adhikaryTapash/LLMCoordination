using System.Text.Json;
using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class SecurityRbacAgent : IAgent
{
    public const string AgentName = "security.rbac";

    private readonly ITenantRepository _tenantRepository;

    public SecurityRbacAgent(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var nlu = GetNluResponse(context);
        var action = nlu?.Intent.Action ?? "read";
        var resource = nlu?.Intent.Resource ?? "unknown";

        if (action.Equals("delete", StringComparison.OrdinalIgnoreCase) && !context.ApprovalGranted)
        {
            return new AgentResult
            {
                AgentName = AgentName,
                Status = AgentStatus.Failed,
                ErrorMessage = "Delete operations require approval.",
                OutputJson = JsonSerializer.Serialize(new { allowed = false, reason = "approval_required" })
            };
        }

        var rules = await _tenantRepository.GetPermissionRulesAsync(context.TenantId, cancellationToken);
        var matchingRule = rules.FirstOrDefault(r =>
            r.Role.Equals(context.UserRole, StringComparison.OrdinalIgnoreCase) &&
            r.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase) &&
            r.Action.Equals(action, StringComparison.OrdinalIgnoreCase));

        var allowed = matchingRule?.Allowed ?? action.Equals("read", StringComparison.OrdinalIgnoreCase);
        var output = new { allowed, action, resource, role = context.UserRole };

        context.AgentOutputs[AgentName] = output;

        return new AgentResult
        {
            AgentName = AgentName,
            Status = allowed ? AgentStatus.Success : AgentStatus.Failed,
            OutputJson = JsonSerializer.Serialize(output),
            ErrorMessage = allowed ? null : $"Role {context.UserRole} cannot perform {action} on {resource}.",
            NextAgentHint = allowed ? "skill.router" : null
        };
    }

    private static NluAgentResponse? GetNluResponse(AgentContext context) =>
        context.AgentOutputs.TryGetValue(NluIntentAgent.AgentName, out var output) && output is NluAgentResponse nlu
            ? nlu
            : null;
}
