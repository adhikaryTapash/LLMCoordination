namespace LLMCoordination.Application.Contracts;

public interface IAgent
{
    Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default);
}
