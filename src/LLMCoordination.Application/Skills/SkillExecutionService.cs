using LLMCoordination.Application.Contracts;

namespace LLMCoordination.Application.Skills;

public class SkillExecutionService
{
    private readonly SkillRegistryService _skillRegistryService;

    public SkillExecutionService(SkillRegistryService skillRegistryService)
    {
        _skillRegistryService = skillRegistryService;
    }

    public async Task<ToolExecutionResult> ExecuteSkillAsync(
        string intentName,
        ToolExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        var skill = await _skillRegistryService.FindBestSkillForIntentAsync(intentName, cancellationToken);
        if (skill is null)
        {
            return new ToolExecutionResult
            {
                Status = "failed",
                RawJson = """{"error":"No matching skill found"}""",
                RecordCount = 0
            };
        }

        return skill.SkillName switch
        {
            "api.query.execute" => new ToolExecutionResult
            {
                Status = "success",
                RawJson = """[{"id":"1001","name":"Mock Patient"}]""",
                RecordCount = 1
            },
            "database.sql.generate" => new ToolExecutionResult
            {
                Status = "success",
                RawJson = """{"sql":"SELECT COUNT(*) FROM patients"}""",
                RecordCount = 1
            },
            _ => new ToolExecutionResult
            {
                Status = "success",
                RawJson = $$"""{"skill":"{{skill.SkillName}}","mock":true}""",
                RecordCount = 0
            }
        };
    }
}
