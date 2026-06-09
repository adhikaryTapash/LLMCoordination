using System.Text.Json;
using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Entities;
using LLMCoordination.Domain.Enums;

namespace LLMCoordination.Application.Agents;

public class EndpointDiscoveryAgent : IAgent
{
    public const string AgentName = "endpoint.discovery";

    private readonly ISwaggerRepository _swaggerRepository;

    public EndpointDiscoveryAgent(ISwaggerRepository swaggerRepository)
    {
        _swaggerRepository = swaggerRepository;
    }

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        var nlu = GetNluResponse(context);
        var message = context.RawMessage.ToLowerInvariant();
        var endpoints = await _swaggerRepository.GetEndpointsAsync(context.TenantId, cancellationToken);

        TenantSwaggerEndpoint? bestMatch = null;
        var bestScore = 0.0;

        foreach (var endpoint in endpoints)
        {
            var score = ScoreEndpoint(endpoint, message, nlu?.Intent.Resource);
            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = endpoint;
            }
        }

        if (bestMatch is null)
        {
            bestMatch = new TenantSwaggerEndpoint
            {
                Method = "GET",
                Path = "/api/patients",
                OperationId = "listPatients",
                Summary = "List patients (mock fallback)"
            };
            bestScore = 0.75;
        }

        var result = new EndpointDiscoveryResult
        {
            Method = bestMatch.Method,
            Path = bestMatch.Path,
            OperationId = bestMatch.OperationId,
            Confidence = bestScore
        };

        context.AgentOutputs[AgentName] = result;

        return new AgentResult
        {
            AgentName = AgentName,
            Status = AgentStatus.Success,
            OutputJson = JsonSerializer.Serialize(result),
            NextAgentHint = "api.execution"
        };
    }

    private static double ScoreEndpoint(
        TenantSwaggerEndpoint endpoint,
        string message,
        string? resource)
    {
        var score = 0.0;
        var pathLower = endpoint.Path.ToLowerInvariant();
        var tagsLower = endpoint.Tags.ToLowerInvariant();
        var summaryLower = endpoint.Summary.ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(resource))
        {
            if (pathLower.Contains(resource, StringComparison.OrdinalIgnoreCase)) score += 0.4;
            if (tagsLower.Contains(resource, StringComparison.OrdinalIgnoreCase)) score += 0.2;
            if (summaryLower.Contains(resource, StringComparison.OrdinalIgnoreCase)) score += 0.2;
        }

        if (message.Contains("patient", StringComparison.OrdinalIgnoreCase) &&
            pathLower.Contains("patient", StringComparison.OrdinalIgnoreCase))
        {
            score += 0.3;
        }

        if (message.Contains("appointment", StringComparison.OrdinalIgnoreCase) &&
            pathLower.Contains("appointment", StringComparison.OrdinalIgnoreCase))
        {
            score += 0.3;
        }

        if (endpoint.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
            message.Contains("list", StringComparison.OrdinalIgnoreCase))
        {
            score += 0.1;
        }

        return Math.Min(score, 1.0);
    }

    private static NluAgentResponse? GetNluResponse(AgentContext context) =>
        context.AgentOutputs.TryGetValue(NluIntentAgent.AgentName, out var output) && output is NluAgentResponse nlu
            ? nlu
            : null;
}
